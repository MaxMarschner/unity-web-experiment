using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;
using TMPro;
using static UnityEngine.ParticleSystem;
using Unity.VisualScripting;
using System.Linq;
//using System.Drawing;

public class ExperimentHandler : MonoBehaviour
{
    #region Declare public variables

    // GameObjects
    public List<GameObject> Agents;
    public TMP_Text Instructions;
    public GameObject KeyboardIllustration;
    public TMP_Text LegendHand;
    public TMP_Text LegendHandPartner;
    public TMP_Text LegendTablet;
    public TMP_Text Cue;
    public GameObject ColorCue;
    public GameObject SquareStim;
    public GameObject CircleStim;
    private Renderer ColorCueRenderer;
    public Color ColorCue_2 = UnityEngine.Color.blue;
    public Color ColorCue_1 = UnityEngine.Color.yellow;
    public GameObject Alarm;
    private Renderer AlarmRenderer;
    public Light AlarmLight;
    public Color AlarmColorOff = UnityEngine.Color.grey;
    public Color AlarmColorOn = UnityEngine.Color.red;
    

    // Text prompts
    public string ErrorMessage = "Error";
    public string HoldPrompt = "Hold";
    public string SlowMessage = "Too Slow";
    public string EarlyMessage = "Too Early";
    private string CatchPrompt = "Attention!\r\nRelease all keys when a person lifts their finger twice.";


    // Animators
    public Animator participantAnimator;
    public Animator partnerAnimator;
    public Animator Avatar1Animator;
    public Animator Avatar2Animator;

    // Experimental Variables
    public int ParticipantID;
    public string HandPosition;
    public string IndexKey = "n";
    public string MiddleKey = "m";
    public string IndexCue = "1";
    public string MiddleCue = "2";
    public GameObject IndexShape;
    public GameObject MiddleShape;

    //public List<string> BlockConditions = new List<string> { "IndInd", "IndGroup", "GroupInd", "GroupGroup" };
    public int PracticeTrialN;
    public int FamTrialN;
    public int TrialN;
    public int CatchTrialN;
    public int CueFontSize;
    public int PromptFontSize;
    public int CatchPromptFontSize;
    public float FixationCrossDelay;
    public float FixationCrossDisplay;
    public float PartnerResponseDelayMin;
    public float PartnerResponseDelayMax;
    public float PartnerRTMin; 
    public float PartnerRTMax;
    public float ImitationDelayMin; 
    public float ImitationDelayMax; 
    public float OutcomeDisplay;
    public float ErrorFeedbackDelay;
    public float ErrorFeedbackDisplay;
    public float CatchFeedbackDisplay;
    public float ITI;
    public float ResponseWindow;
    public float ResponseWindowCatch;
    
    private float referenceTime;
    private int blockCounter;
    private string blockOrder;
    private string blockCondition;
    private string SRMapping;
    private string firstImitator;

    // Balanced lating square design for condition orders
    private List<List<string>> conditionOrders = new List<List<string>>
    {
        new List<string> { "IndInd", "IndGroup", "GroupGroup", "GroupInd" },
        new List<string> { "IndGroup", "GroupInd", "IndInd", "GroupGroup" },
        new List<string> { "GroupInd", "GroupGroup", "IndGroup", "IndInd" },
        new List<string> { "GroupGroup", "IndInd", "GroupInd", "IndGroup" }
    };

    // Data structures

    // Trial data
    [System.Serializable]
    public class TrialData
    {
        public int ParticipantID;
        public string HandPosition;
        public int blockCounter;
        public string blockCondition;
        public string blockOrder; 
        public string SRMapping;
        public string firstImitator;
        public int trialCounter;
        public int trialCondition;
        public string imitatingAvatar;
        public string response;
        public float responseTime;
        public bool responseCorrect;
        public bool catchTrialResponse;
        public float responseDelayPartner;
        public float returnDelayPartner;
        public float responseTimePartner;
        public float imitationDelayAvatar1;
        public float imitationDelayAvatar2;
        public float returnDelayAvatar1;
        public float returnDelayAvatar2;


        // for senity check
        public override string ToString()
        {
            return $"ParticipantID: {ParticipantID}, Block Counter: {blockCounter}, " +
                   $"Block Condition: {blockCondition}, Trial Counter: {trialCounter}, " +
                   $"Trial Condition: {trialCondition}, Response: {response}, " +
                   $"Response Time: {responseTime}, Response Time Partner: {responseTimePartner}, Response Correct: {responseCorrect}, " +
                   $"Imitation Delay Avatar1: {imitationDelayAvatar1}, Imitation Delay Avatar2: {imitationDelayAvatar2}," +
                   $"Return Delay Avatar1: {returnDelayAvatar1}, Return Delay Avatar2: {returnDelayAvatar2},";
        }

    }

    [System.Serializable]
    public class TrialDataList
    {
        public List<TrialData> trialDataList;
    }

    // Experiment data
    public List<TrialData> experimentData = new List<TrialData>();

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Set experimental variables
        ParticipantID = GenerateParticipantID(8);
        HandPosition = "right";
        PracticeTrialN = 8;
        FamTrialN = 2;
        TrialN = 36;
        CatchTrialN = 4;
        CueFontSize = 36;
        PromptFontSize = 24;
        CatchPromptFontSize = 11;
        FixationCrossDelay = 0.5f;
        FixationCrossDisplay = 1f;
        PartnerResponseDelayMin = 0f;
        PartnerResponseDelayMax = 0.02f;
        PartnerRTMin = 0.420f; // adjust to Pfister & Weller et al. (2017) or pilot data
        PartnerRTMax = 0.470f;
        ImitationDelayMin = 0.200f; // 0.319f; // from Pfister & Weller et al. (2017)
        ImitationDelayMax = 0.220f; // 0.443f; // alternative: Cross & Iacoboni 2014b --> 521ms mean for prepaired imitative finger lift responses
        OutcomeDisplay = 1f;
        ErrorFeedbackDelay = 0.1f;
        ErrorFeedbackDisplay = 1.5f;
        CatchFeedbackDisplay = 5.0f;
        ITI = 0.5f;
        ResponseWindow = 1.5f;
        ResponseWindowCatch = 0.5f;
        ColorCue_2 = UnityEngine.Color.blue; // changed 23.09.2024 to see if color drives main effect of response in experiment 1 --> adapted instructions accordingly
        ColorCue_1 = UnityEngine.Color.yellow; // changed 23.09.2024 to see if color drives main effect of response in experiment 1

        // Initialize screen display
        KeyboardIllustration.SetActive(false);
        Cue.enabled = false;
        ColorCue.SetActive(false);
        CircleStim.SetActive(false);
        SquareStim.SetActive(false);
        ColorCueRenderer = ColorCue.GetComponent<Renderer>();
        AlarmLight.enabled = false;
        AlarmRenderer = Alarm.GetComponent<Renderer>();
        AlarmRenderer.material.color = AlarmColorOff;

        StartCoroutine(ExpProcedure());
        //StartCoroutine(TrialProcedure());
    }

    // Update is called once per frame
    void Update()
    {
    }

    #region Instructions



    private string InstructionText_I = "<u><b>Instructions</b></u>\r\n\r\nYour task in this experiment is to respond to different shapes (square and circle) as they appear on screen by lifting the index or middle finger of your right hand. \r\n\r\nPlace your <b>right index finger</b> on the <b>[N]</b>, and your <b>right middle finger</b> on the <b>[M]</b> key for the whole experiment! \r\n[see illustration]<b>→</b>\r\n\r\nPress [space] to continue.";
    private string InstructionText_II = "<u><b>Instructions</b></u>\r\n\r\nYou will perform the task in a virtual environment.\r\n\r\nYour right hand is represented by a virtual hand that is going to mimic your finger lift responses. The shapes will be displayed on the small tablet screen.\r\n\r\nPress [space] to continue.";
    private string InstructionText_IV = "<u><b>Instructions</b></u>\r\n\r\nWhen you hold the [N] and the [M] key depressed, a cross will appear on the tablet screen. \r\n\r\n<b>Hold both keys depressed until a <b>circle</b> or a <b>square</b> appears on the tablet screen.</b> If you release one of the keys too early, the trial will start all over again.  \r\n\r\nPress [space] to continue.";

    private string InstructionText_III_A = "<u><b>Instructions</b></u>\r\n\r\nYour job is to respond to <b>circles</b> by <b>releasing</b> the <b>[N]</b> key and to <b>squares</b> by <b>releasing</b> the <b>[M]</b> key.\r\n\r\nAt the start of each trial, you will see a \"Hold\" prompt presented on the tablet screen. When you see this prompt, ensure that you <b>hold the [N] and the [M] key depressed.</b>\r\n\r\nPress [space] to continue.";
    private string InstructionText_V_A = "<u><b>Instructions</b></u>\r\n\r\nAs soon as one of the shapes appears on the tablet screen, release the [N] or [M] key as quickly as you can by lifting your index or middle finger.\r\n\r\n<b>Respond to the shapes as follows:</b>\r\n<b>Circle</b>: lift your <b>index finger</b> \r\n<b>Square</b>: lift your <b>middle finger</b>\r\n\r\nPress [space] to try it out.";
    private string InstructionTextPracticeInd_A = "<u><b>Try it out:</b></u>\r\n\r\n<b>Hold the [N] and [M] key depressed</b> until a shape appears on the tablet screen.\r\n\r\n<b>Respond to the shapes as follows:</b>\r\n<b>Circle</b>: lift your <b>index finger</b>\r\n<b>Square</b>: lift your <b>middle finger</b>";
    private string InstructionTextPracticeJoint_A = "<u><b>Instructions</b></u>\r\n\r\n<b>From now on you will perform the task together with a virtual partner</b>. Rather than responding alone, <b>you and your partner will now respond to the shapes together.</b>\r\n\r\nThe task remains the same:\r\n<b>Circle</b>: lift your <b>index finger</b>\r\n<b>Square:</b> lift your <b>middle finger</b>\r\n\r\nPress [space] to try it out.";

    private string InstructionText_III_B = "<u><b>Instructions</b></u>\r\n\r\nYour job is to respond to <b>squares</b> by <b>releasing</b> the <b>[N]</b> key and to <b>circles</b> by <b>releasing</b> the <b>[M]</b> key.\r\n\r\nAt the start of each trial, you will see a \"Hold\" prompt presented on the tablet screen. When you see this prompt, ensure that you <b>hold the [N] and the [M] key depressed.</b>\r\n\r\nPress [space] to continue.";
    private string InstructionText_V_B = "<u><b>Instructions</b></u>\r\n\r\nAs soon as one of the shapes appears on the tablet screen, release the [N] or [M] key as quickly as you can by lifting your index or middle finger.\r\n\r\n<b>Respond to the shapes as follows:</b>\r\n<b>Square</b>: lift your <b>index finger</b> \r\n<b>Circle</b>: lift your <b>middle finger</b>\r\n\r\nPress [space] to try it out.";
    private string InstructionTextPracticeInd_B = "<u><b>Try it out:</b></u>\r\n\r\n<b>Hold the [N] and [M] key depressed</b> until a shape appears on the tablet screen.\r\n\r\n<b>Respond to the shapes as follows:</b>\r\n<b>Square</b>: lift your <b>index finger</b>\r\n<b>Circle</b>: lift your <b>middle finger</b>";
    private string InstructionTextPracticeJoint_B = "<u><b>Instructions</b></u>\r\n\r\n<b>From now on you will perform the task together with a virtual partner</b>. Rather than responding alone, <b>you and your partner will now respond to the shapes together.</b>\r\n\r\nThe task remains the same:\r\n<b>Square</b>: lift your <b>index finger</b>\r\n<b>Circle:</b> lift your <b>middle finger</b>\r\n\r\nPress [space] to try it out.";


    private string InstructionTextImitation_I = "<u><b>Instructions</b></u>\r\n\r\nFrom now on, two other people \r\nwill observe you and your partner performing the task. \r\n\r\n<b>Their task is to imitate your finger lift responses</b>. Both won't be able to see the shapes presented on the tablet screen. They will wait for you to respond, and then copy your index or middle finger movement. If you make an error, a red light will signal them to withold their \r\nresponse.\r\n\r\nPress [space] to continue.";
    private string InstructionTextImitation_III = "<u><b>Instructions</b></u>\r\n\r\nThe remainder of the experiment is structured \r\ninto four parts. \r\n\r\nIn the seperate parts, you will perform the task <b>either alone or together with your partner</b> and <b>either one or both of the people observing you will imitate your responses.</b>\r\n\r\nPress [space] to start.";
    private string InstructionTextImitation_II = "<u><b>Instructions</b></u>\r\n\r\nFrom now on, <b>pay attention \r\nto the responses of the two people sitting in front of you!</b> \r\n\r\nOn some trials, they will imitate your responses <u><b>by lifting their index or middle finger twice</u>.</b> <b>When you notice this behavior, <u>quickly release both of your fingers from your keyboard!</u></b>\r\n\r\nPress [space] to continue.";
    
    private string InstructionTextIndInd = "</b></u>\r\n\r\nIn this part, <b>you will perform the task on your own</b> while your partner remains passive, and <b>one of the persons observing you will imitate your responses</b>.";
    private string InstructionTextIndGroup = "</b></u>\r\n\r\nIn this part, <b>you will perform the task on your own</b> while your partner remains passive, and <b>both of the persons observing you will imitate your responses</b>.\r\n\r\nYou will start with two training trials to get familiar with the procedure.\r\n\r\nPress [space] to start.";
    private string InstructionTextGroupInd = "</b></u>\r\n\r\nIn this part, <b>you will perform the task together with your partner</b>, and <b>one of the persons observing you will imitate your responses</b>.";
    private string InstructionTextGroupGroup = "</b></u>\r\n\r\nIn this part, <b>you will perform the task together with your partner</b>, and <b>both of the persons observing you will imitate your responses</b>.\r\n\r\nYou will start with two training trials to get familiar with the procedure.\r\n\r\nPress [space] to start.";
    private string InstructionTextTest = "\r\n\r\n\r\n\r\nAll set!\r\n\r\nPress [space] to continue with the actual task!";
    private string partText = "";
    private string ImitatingAvatarInstructionsTextFirstHalf = "";
    private string ImitatingAvatarInstructionsTextSecondHalf = "";
    private string EndText = "\r\n\r\n\r\n\r\n<b>You have reached the end of the experiment!</b>\r\n\r\nPress [space] to continue.";

    #endregion

    #region Define Coroutines
    IEnumerator TaskInstructions()
    {
        // Init scene
        Agents[0].SetActive(false); // Participant Hand
        Agents[1].SetActive(false); // Partner Hand
        Agents[2].SetActive(false); // Avatar1
        Agents[3].SetActive(false); // Avatar2
        LegendHand.enabled = false;
        LegendHandPartner.enabled = false;
        LegendTablet.enabled = false;

        // Show instructions
        Instructions.text = InstructionText_I;
        Instructions.enabled = true;
        KeyboardIllustration.SetActive(true);
        yield return new WaitUntil(() => Input.GetKey("space"));
        Instructions.enabled = false;
        KeyboardIllustration.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        Instructions.text = InstructionText_II;
        Agents[0].SetActive(true); // Participant Hand
        Instructions.enabled = true;
        LegendHand.enabled = true;
        LegendTablet.enabled = true;
        yield return new WaitUntil(() => Input.GetKey("space"));
        Instructions.enabled = false;
        yield return new WaitForSeconds(0.5f);

        if (SRMapping == "A")
        {
            Instructions.text = InstructionText_III_A;
            Instructions.enabled = true;
            Cue.text = HoldPrompt;
            Cue.fontSize = PromptFontSize;
            Cue.enabled = true;
            LegendHand.enabled = false;
            LegendTablet.enabled = false;
            yield return new WaitUntil(() => Input.GetKey("space"));
            Instructions.enabled = false;
            Cue.enabled = false;
            yield return new WaitForSeconds(0.5f);
        } 
        else if (SRMapping == "B")
        {
            Instructions.text = InstructionText_III_B;
            Instructions.enabled = true;
            Cue.text = HoldPrompt;
            Cue.fontSize = PromptFontSize;
            Cue.enabled = true;
            LegendHand.enabled = false;
            LegendTablet.enabled = false;
            yield return new WaitUntil(() => Input.GetKey("space"));
            Instructions.enabled = false;
            Cue.enabled = false;
            yield return new WaitForSeconds(0.5f);
        }

        Instructions.text = InstructionText_IV;
        Instructions.enabled = true;
        Cue.text = "+";
        Cue.fontSize = CueFontSize;
        Cue.enabled = true;
        yield return new WaitUntil(() => Input.GetKey("space"));
        Instructions.enabled = false;
        Cue.enabled = false;
        yield return new WaitForSeconds(0.5f);

        if (SRMapping == "A")
        {
            Instructions.text = InstructionText_V_A;
            Instructions.enabled = true;
            Cue.text = IndexCue;
            //ColorCue.SetActive(true);
            //SquareStim.SetActive(true);
            IndexShape.SetActive(true);
            Cue.fontSize = CueFontSize;
            Cue.enabled = true;
            yield return new WaitUntil(() => Input.GetKey("space"));
            Instructions.enabled = false;
            Cue.enabled = false;
            //ColorCue.SetActive(false);
            //SquareStim.SetActive(false);
            IndexShape.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        } 
        else if (SRMapping == "B")
        {
            Instructions.text = InstructionText_V_B;
            Instructions.enabled = true;
            Cue.text = IndexCue;
            //ColorCue.SetActive(true);
            //SquareStim.SetActive(true);
            IndexShape.SetActive(true);
            Cue.fontSize = CueFontSize;
            Cue.enabled = true;
            yield return new WaitUntil(() => Input.GetKey("space"));
            Instructions.enabled = false;
            Cue.enabled = false;
            //ColorCue.SetActive(false);
            //SquareStim.SetActive(false);
            IndexShape.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }
        
    }

    IEnumerator PracticeIndTrials(int PracticeTrialN) // Solo practice
    {
        // Activate avatars
        Agents[0].SetActive(true); // Participant Hand
        Agents[1].SetActive(false); // Partner Hand
        Agents[2].SetActive(false); // Avatar1
        Agents[3].SetActive(false); // Avatar2

        // Show instructions
        if (SRMapping == "A")
        {
            Instructions.text = InstructionTextPracticeInd_A;
        }
        else if (SRMapping == "B")
        {
            Instructions.text = InstructionTextPracticeInd_B;
        }

        Instructions.enabled = true;
        LegendHand.enabled = false;
        LegendHandPartner.enabled = false;
        LegendTablet.enabled = false;

        int[] trialListPracticeInd = trialRandomizer(PracticeTrialN, 0); // randomize trial conditions

        for (int i = 0; i < PracticeTrialN; i++)
        {
            // init TrialData class to save trial data
            TrialData thisTrial = new TrialData();
            thisTrial.ParticipantID = ParticipantID;
            thisTrial.HandPosition = HandPosition;
            thisTrial.blockCounter = 0;
            thisTrial.blockCondition = "PracticeInd";
            thisTrial.blockOrder = blockOrder;
            thisTrial.SRMapping = SRMapping;

            // Init trial variables
            thisTrial.trialCounter = i + 1;
            thisTrial.response = null;
            thisTrial.responseTime = 0;
            thisTrial.responseCorrect = false;

            // Display hold prompt and wait until both keys are pressed
            Cue.text = HoldPrompt;
            Cue.fontSize = PromptFontSize;
            Cue.enabled = true;
            yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

            // Display fixation cross after delay and listen for key release before cue onset
            referenceTime = Time.time;
            while ((Time.time - referenceTime) < (FixationCrossDelay + FixationCrossDisplay)) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
            {
                yield return null;
                if (!Input.GetKey(IndexKey) || !Input.GetKey(MiddleKey))
                {
                    thisTrial.response = "early";
                    break;
                }
                if (Time.time - referenceTime >= FixationCrossDelay)
                {
                    Cue.fontSize = CueFontSize;
                    Cue.text = "+";
                }
            }

            // If response was made before cue presentation --> skip/repeat trial
            if (thisTrial.response == "early")
            {
                i--; // repeat trial --> double check if this works as intended!
                Cue.fontSize = PromptFontSize;
                Cue.text = EarlyMessage;
                Cue.enabled = true;
                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                // Display hold prompt
                Cue.text = HoldPrompt;
                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
            }

            // If no response was made before cue presentation --> proceed with trial structure 
            else if (thisTrial.response == null)
            {
                // Case: Cue = 1
                if (trialListPracticeInd[i] == 1)
                {
                    // Display cue and wait for key release
                    //ColorCueRenderer.material.color = ColorCue_1;
                    //ColorCue.SetActive(true);
                    //CircleStim.SetActive(true);
                    IndexShape.SetActive(true);
                    Cue.text = IndexCue;
                    thisTrial.trialCondition = 1;

                    // set reference time
                    referenceTime = Time.time;
                    while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                    {
                        yield return null;
                        if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // correct response
                        {
                            // record/update response data
                            thisTrial.response = IndexKey;
                            thisTrial.responseCorrect = true;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //CircleStim.SetActive(false);
                            IndexShape.SetActive(false);
                            Debug.Log("Response: " + thisTrial.response);
                            Debug.Log("Response correct: " + thisTrial.responseCorrect);
                            Debug.Log("RT: " + thisTrial.responseTime);

                            // Trigger WE finger lift
                            participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift
                            yield return new WaitForSeconds(OutcomeDisplay);

                            // Reset animation transitions
                            participantAnimator.SetInteger("liftFinger", 0);

                            // Display hold prompt
                            Cue.fontSize = PromptFontSize;
                            Cue.text = HoldPrompt;
                            Cue.enabled = true;

                            // Wait for key press and trigger WE finger return animation
                            yield return new WaitUntil(() => Input.GetKey(IndexKey));
                            participantAnimator.SetBool("return", true);

                            // Reset animation transitions
                            yield return null;
                            participantAnimator.SetBool("return", false);

                            break;
                        }
                        else if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // wrong response
                        {
                            i--; // repeat training trial when error was made

                            // record/update response data
                            thisTrial.response = MiddleKey;
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //CircleStim.SetActive(false);
                            IndexShape.SetActive(false);

                            // Trigger WE finger lift
                            participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift
                            yield return new WaitForSeconds(ErrorFeedbackDelay); // delay error feedback

                            // Display error feedback
                            participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                            AlarmRenderer.material.color = AlarmColorOn; // light turns red
                            AlarmLight.enabled = true;
                            Cue.fontSize = PromptFontSize;
                            Cue.text = ErrorMessage;
                            Cue.enabled = true;
                            yield return new WaitForSeconds(ErrorFeedbackDisplay);

                            // Display hold prompt
                            AlarmRenderer.material.color = AlarmColorOff;
                            AlarmLight.enabled = false;
                            Cue.text = HoldPrompt;

                            // Wait for key press and trigger WE finger return animation
                            yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                            participantAnimator.SetBool("return", true);

                            // Reset animation transitions
                            yield return null;
                            participantAnimator.SetBool("return", false);

                            break;
                        }
                        else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                        {
                            i--; // repeat training trial when error was made

                            // record/update response data
                            thisTrial.response = "double";
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //CircleStim.SetActive(false);
                            IndexShape.SetActive(false);

                            // Display error feedback
                            participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                            AlarmRenderer.material.color = AlarmColorOn; // light turns red
                            AlarmLight.enabled = true;
                            Cue.fontSize = PromptFontSize;
                            Cue.text = ErrorMessage;
                            Cue.enabled = true;
                            yield return new WaitForSeconds(ErrorFeedbackDisplay);

                            // Display hold prompt
                            AlarmRenderer.material.color = AlarmColorOff;
                            AlarmLight.enabled = false;
                            Cue.text = HoldPrompt;

                            yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                            break;
                        }
                        else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                        {
                            thisTrial.response = "none";
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = 0;
                        }
                    }
                    // if no response was recorded during response window, display error message
                    if (thisTrial.response == "none")
                    {
                        i--; // repeat training trial when error was made

                        Cue.fontSize = PromptFontSize;
                        Cue.text = SlowMessage;
                        Cue.enabled = true;
                        //ColorCue.SetActive(false);
                        //CircleStim.SetActive(false);
                        IndexShape.SetActive(false);
                        yield return new WaitForSeconds(ErrorFeedbackDisplay);

                        // Display hold prompt
                        AlarmRenderer.material.color = AlarmColorOff;
                        AlarmLight.enabled = false;
                        Cue.text = HoldPrompt;

                        yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                    }
                }
                else if (trialListPracticeInd[i] == 2)
                {
                    //ColorCueRenderer.material.color = ColorCue_2;
                    //ColorCue.SetActive(true);
                    //SquareStim.SetActive(true);
                    MiddleShape.SetActive(true);
                    Cue.text = MiddleCue;
                    thisTrial.trialCondition = 2;

                    referenceTime = Time.time;
                    while ((Time.time - referenceTime) <= ResponseWindow) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
                    {
                        yield return null;
                        if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                        {
                            thisTrial.response = MiddleKey;
                            thisTrial.responseCorrect = true;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //SquareStim.SetActive(false);
                            MiddleShape.SetActive(false);
                            Debug.Log("Response: " + thisTrial.response);
                            Debug.Log("Response correct: " + thisTrial.responseCorrect);
                            Debug.Log("RT: " + thisTrial.responseTime);

                            // Trigger WE finger lift
                            participantAnimator.SetInteger("liftFinger", 2);
                            yield return new WaitForSeconds(OutcomeDisplay); // Duration outcome display

                            // Reset animation transitions
                            participantAnimator.SetInteger("liftFinger", 0);

                            // Display hold prompt
                            Cue.fontSize = PromptFontSize;
                            Cue.text = HoldPrompt;
                            Cue.enabled = true;

                            // Wait for key press and trigger WE finger return animation
                            yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                            participantAnimator.SetBool("return", true);

                            // Reset animation transitions
                            yield return null;
                            participantAnimator.SetBool("return", false);

                            break;
                        }
                        else if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey))
                        {
                            i--; // repeat training trial when error was made

                            thisTrial.response = IndexKey;
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //SquareStim.SetActive(false);
                            MiddleShape.SetActive(false);

                            // Trigger WE finger lift
                            participantAnimator.SetInteger("liftFinger", 1);
                            yield return new WaitForSeconds(ErrorFeedbackDelay);

                            // Display error feedback
                            participantAnimator.SetInteger("liftFinger", 0);
                            AlarmRenderer.material.color = AlarmColorOn;
                            AlarmLight.enabled = true;
                            Cue.fontSize = PromptFontSize;
                            Cue.text = ErrorMessage;
                            Cue.enabled = true;
                            yield return new WaitForSeconds(ErrorFeedbackDisplay);

                            // Display hold prompt
                            AlarmRenderer.material.color = AlarmColorOff;
                            AlarmLight.enabled = false;
                            Cue.text = HoldPrompt;

                            // Wait for key press and trigger WE finger return animation
                            yield return new WaitUntil(() => Input.GetKey(IndexKey));
                            participantAnimator.SetBool("return", true);

                            // Reset animation transitions
                            yield return null;
                            participantAnimator.SetBool("return", false);

                            break;
                        }
                        else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey))
                        {
                            i--; // repeat training trial when error was made

                            thisTrial.response = "double";
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //SquareStim.SetActive(false);
                            MiddleShape.SetActive(false);

                            // Display error feedback
                            participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                            AlarmRenderer.material.color = AlarmColorOn; // light turns red
                            AlarmLight.enabled = true;
                            Cue.fontSize = PromptFontSize;
                            Cue.text = ErrorMessage;
                            Cue.enabled = true;
                            yield return new WaitForSeconds(ErrorFeedbackDisplay);

                            // Display hold prompt
                            AlarmRenderer.material.color = AlarmColorOff;
                            AlarmLight.enabled = false;
                            Cue.text = HoldPrompt;

                            yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                        }
                        else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                        {
                            thisTrial.response = "none";
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = 0;
                        }
                    }

                    if (thisTrial.response == "none")
                    {
                        i--; // repeat training trial when error was made

                        Cue.fontSize = PromptFontSize;
                        Cue.text = SlowMessage;
                        Cue.enabled = true;
                        //ColorCue.SetActive(false);
                        //SquareStim.SetActive(false);
                        MiddleShape.SetActive(false);
                        yield return new WaitForSeconds(ErrorFeedbackDisplay);

                        // Display hold prompt
                        AlarmRenderer.material.color = AlarmColorOff;
                        AlarmLight.enabled = false;
                        Cue.text = HoldPrompt;

                        yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                    }
                }
            }
            experimentData.Add(thisTrial);
            yield return new WaitForSeconds(ITI); // ITI
        }
        Instructions.enabled = false;
        Cue.enabled = false;
        //ColorCue.SetActive(false);
        //CircleStim.SetActive(false);
        //SquareStim.SetActive(false);
        IndexShape.SetActive(false);
        MiddleShape.SetActive(false);
        yield return new WaitForSeconds(ITI);
    }

    IEnumerator PracticeGroupTrials(int PracticeTrialN) // Group practice 
    {
        // Activate avatars
        Agents[0].SetActive(true); // Participant Hand
        Agents[1].SetActive(true); // Partner Hand
        Agents[2].SetActive(false); // Avatar1
        Agents[3].SetActive(false); // Avatar2
        LegendHand.enabled = false;
        LegendHandPartner.enabled = false;
        LegendTablet.enabled = false;

        // Show instructions
        if (SRMapping == "A")
        {
            Instructions.text = InstructionTextPracticeJoint_A;
        }
        else if (SRMapping == "B")
        {
            Instructions.text = InstructionTextPracticeJoint_B;
        }
        Instructions.enabled = true;
        LegendHand.enabled = true;
        LegendHandPartner.enabled = true;
        yield return new WaitUntil(() => Input.GetKey("space"));
        Instructions.enabled = false;
        yield return new WaitForSeconds(0.5f);

        if (SRMapping == "A")
        {
            Instructions.text = InstructionTextPracticeInd_A;
        }
        else if (SRMapping == "B")
        {
            Instructions.text = InstructionTextPracticeInd_B;
        }
        Instructions.enabled = true;
        LegendHand.enabled = false;
        LegendHandPartner.enabled = false;

        int[] trialListPracticeGroup = trialRandomizer(PracticeTrialN, 0); // randomize trial conditions

        for (int i = 0; i < PracticeTrialN; i++)
        {
            // init TrialData class to save trial data
            TrialData thisTrial = new TrialData();
            thisTrial.ParticipantID = ParticipantID;
            thisTrial.HandPosition = HandPosition;
            thisTrial.blockCounter = 0;
            thisTrial.blockCondition = "PracticeGroup";
            thisTrial.blockOrder = blockOrder;
            thisTrial.SRMapping = SRMapping;
            thisTrial.responseDelayPartner = UnityEngine.Random.Range(PartnerResponseDelayMin, PartnerResponseDelayMax);
            thisTrial.returnDelayPartner = UnityEngine.Random.Range(PartnerResponseDelayMin, PartnerResponseDelayMax);

            // Init trial variables
            thisTrial.trialCounter = i + 1;
            thisTrial.response = null;
            thisTrial.responseTime = 0;
            thisTrial.responseCorrect = false;

            // Display hold prompt and wait until both keys are pressed
            Cue.text = HoldPrompt;
            Cue.fontSize = PromptFontSize;
            Cue.enabled = true;
            yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

            // Display fixation cross after delay and listen for key release before cue onset
            referenceTime = Time.time;
            while ((Time.time - referenceTime) < (FixationCrossDelay + FixationCrossDisplay)) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
            {
                yield return null;
                if (!Input.GetKey(IndexKey) || !Input.GetKey(MiddleKey))
                {
                    thisTrial.response = "early";
                    break;
                }
                if (Time.time - referenceTime >= FixationCrossDelay)
                {
                    Cue.fontSize = CueFontSize;
                    Cue.text = "+";
                }
            }

            // If response was made before cue presentation --> skip/repeat trial
            if (thisTrial.response == "early")
            {
                i--; // repeat trial --> double check if this works as intended!
                Cue.fontSize = PromptFontSize;
                Cue.text = EarlyMessage;
                Cue.enabled = true;
                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                // Display hold prompt
                Cue.text = HoldPrompt;
                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
            }

            // If no response was made before cue presentation --> proceed with trial structure 
            else if (thisTrial.response == null)
            {
                // Case: Cue = 1
                if (trialListPracticeGroup[i] == 1)
                {
                    // Display cue and wait for key release
                    //ColorCueRenderer.material.color = ColorCue_1;
                    //ColorCue.SetActive(true);
                    //CircleStim.SetActive(true);
                    IndexShape.SetActive(true);
                    Cue.text = IndexCue;
                    thisTrial.trialCondition = 1;

                    referenceTime = Time.time;
                    while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                    {
                        yield return null;
                        if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // correct response
                        {
                            // record/update response data
                            thisTrial.response = IndexKey;
                            thisTrial.responseCorrect = true;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //CircleStim.SetActive(false);
                            IndexShape.SetActive(false);
                            Debug.Log("Response: " + thisTrial.response);
                            Debug.Log("Response correct: " + thisTrial.responseCorrect);
                            Debug.Log("RT: " + thisTrial.responseTime);

                            // Trigger participant finger lift
                            participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift

                            // Trigger partner finger lift after variable delay
                            yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                            partnerAnimator.SetInteger("liftFinger", 1);

                            // Show outcome
                            yield return new WaitForSeconds(OutcomeDisplay);

                            // Reset animation transitions
                            participantAnimator.SetInteger("liftFinger", 0);
                            partnerAnimator.SetInteger("liftFinger", 0);

                            // Display hold prompt
                            Cue.fontSize = PromptFontSize;
                            Cue.text = HoldPrompt;
                            Cue.enabled = true;

                            // Wait for key press and trigger WE finger return animation
                            yield return new WaitUntil(() => Input.GetKey(IndexKey));
                            participantAnimator.SetBool("return", true);
                            yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                            partnerAnimator.SetBool("return", true);

                            // Reset animation transitions
                            yield return null;
                            participantAnimator.SetBool("return", false);
                            partnerAnimator.SetBool("return", false);

                            break;
                        }
                        else if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // wrong response
                        {
                            i--; // repeat training trial when error was made

                            // record/update response data
                            thisTrial.response = MiddleKey;
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //CircleStim.SetActive(false);
                            IndexShape.SetActive(false);

                            // Trigger WE finger lift
                            participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift
                            yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                            partnerAnimator.SetInteger("liftFinger", 1);

                            // Display error feedback
                            AlarmRenderer.material.color = AlarmColorOn; // light turns red
                            AlarmLight.enabled = true;
                            Cue.fontSize = PromptFontSize;
                            Cue.text = ErrorMessage;
                            Cue.enabled = true;
                            yield return new WaitForSeconds(ErrorFeedbackDisplay);

                            // Reset animation transitions
                            participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                            partnerAnimator.SetInteger("liftFinger", 0);

                            // Display hold prompt
                            AlarmRenderer.material.color = AlarmColorOff;
                            AlarmLight.enabled = false;
                            Cue.text = HoldPrompt;

                            // Wait for key press and trigger WE finger return animation
                            yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                            participantAnimator.SetBool("return", true);
                            yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                            partnerAnimator.SetBool("return", true);

                            // Reset animation transitions
                            yield return null;
                            participantAnimator.SetBool("return", false);
                            partnerAnimator.SetBool("return", false);

                            break;
                        }
                        else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                        {
                            i--; // repeat training trial when error was made

                            // record/update response data
                            thisTrial.response = "double";
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //CircleStim.SetActive(false);
                            IndexShape.SetActive(false);

                            // Trigger partner finger lift
                            yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                            partnerAnimator.SetInteger("liftFinger", 1);

                            // Display error feedback
                            AlarmRenderer.material.color = AlarmColorOn; // light turns red
                            AlarmLight.enabled = true;
                            Cue.fontSize = PromptFontSize;
                            Cue.text = ErrorMessage;
                            Cue.enabled = true;
                            yield return new WaitForSeconds(ErrorFeedbackDisplay);

                            // Reset animation transitions
                            participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                            partnerAnimator.SetInteger("liftFinger", 0);

                            // Display hold prompt
                            AlarmRenderer.material.color = AlarmColorOff;
                            AlarmLight.enabled = false;
                            Cue.text = HoldPrompt;

                            // Wait for key press and trigger partner finger return animation
                            yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                            yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                            partnerAnimator.SetBool("return", true);

                            // Reset animation transitions
                            yield return null;
                            partnerAnimator.SetBool("return", false);

                            break;
                        }
                        else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                        {
                            thisTrial.response = "none";
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = 0;
                        }
                    }
                    // if no response was recorded during response window, display error message
                    if (thisTrial.response == "none")
                    {
                        /* TBD --> should partner respond when participants omit response?
                        // Trigger partner finger lift
                        partnerAnimator.SetInteger("liftFinger", 1);
                        yield return new WaitForSeconds(ErrorFeedbackDelay); // delay slow feedback
                        partnerAnimator.SetInteger("liftFinger", 0); // reset animation transition
                        */
                        i--; // repeat training trial when error was made

                        Cue.fontSize = PromptFontSize;
                        Cue.text = SlowMessage;
                        Cue.enabled = true;
                        //ColorCue.SetActive(false);
                        //CircleStim.SetActive(false);
                        IndexShape.SetActive(false);
                        yield return new WaitForSeconds(ErrorFeedbackDisplay);

                        // Display hold prompt
                        AlarmRenderer.material.color = AlarmColorOff;
                        AlarmLight.enabled = false;
                        Cue.text = HoldPrompt;

                        yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                        /* TBD --> should partner respond when participants omit response?
                        // Wait for key press and trigger partner finger return animation
                        yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                        yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                        partnerAnimator.SetBool("return", true);

                        // Reset animation transitions
                        yield return null;
                        partnerAnimator.SetBool("return", false);
                        */
                    }
                }
                else if (trialListPracticeGroup[i] == 2)
                {
                    // Display cue and wait for key release
                    //ColorCueRenderer.material.color = ColorCue_2;
                    //ColorCue.SetActive(true);
                    //SquareStim.SetActive(true);
                    MiddleShape.SetActive(true);
                    Cue.text = MiddleCue;
                    thisTrial.trialCondition = 2;

                    referenceTime = Time.time;
                    while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                    {
                        yield return null;
                        if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // correct response
                        {
                            // record/update response data
                            thisTrial.response = MiddleKey;
                            thisTrial.responseCorrect = true;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //SquareStim.SetActive(false);
                            MiddleShape.SetActive(false);
                            Debug.Log("Response: " + thisTrial.response);
                            Debug.Log("Response correct: " + thisTrial.responseCorrect);
                            Debug.Log("RT: " + thisTrial.responseTime);

                            // Trigger participant finger lift
                            participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift

                            // Trigger partner finger lift after variable delay
                            yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                            partnerAnimator.SetInteger("liftFinger", 2);

                            // Show outcome
                            yield return new WaitForSeconds(OutcomeDisplay);

                            // Reset animation transitions
                            participantAnimator.SetInteger("liftFinger", 0);
                            partnerAnimator.SetInteger("liftFinger", 0);

                            // Display hold prompt
                            Cue.fontSize = PromptFontSize;
                            Cue.text = HoldPrompt;
                            Cue.enabled = true;

                            // Wait for key press and trigger WE finger return animation
                            yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                            participantAnimator.SetBool("return", true);
                            yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                            partnerAnimator.SetBool("return", true);

                            // Reset animation transitions
                            yield return null;
                            participantAnimator.SetBool("return", false);
                            partnerAnimator.SetBool("return", false);

                            break;
                        }
                        else if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // wrong response
                        {
                            i--; // repeat training trial when error was made

                            // record/update response data
                            thisTrial.response = IndexKey;
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //SquareStim.SetActive(false);
                            MiddleShape.SetActive(false);

                            // Trigger WE finger lift
                            participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift
                            yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                            partnerAnimator.SetInteger("liftFinger", 2);

                            // Display error feedback
                            AlarmRenderer.material.color = AlarmColorOn; // light turns red
                            AlarmLight.enabled = true;
                            Cue.fontSize = PromptFontSize;
                            Cue.text = ErrorMessage;
                            Cue.enabled = true;
                            yield return new WaitForSeconds(ErrorFeedbackDisplay);

                            // Reset animation transitions
                            participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                            partnerAnimator.SetInteger("liftFinger", 0);

                            // Display hold prompt
                            AlarmRenderer.material.color = AlarmColorOff;
                            AlarmLight.enabled = false;
                            Cue.text = HoldPrompt;

                            // Wait for key press and trigger WE finger return animation
                            yield return new WaitUntil(() => Input.GetKey(IndexKey));
                            participantAnimator.SetBool("return", true);
                            yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                            partnerAnimator.SetBool("return", true);

                            // Reset animation transitions
                            yield return null;
                            participantAnimator.SetBool("return", false);
                            partnerAnimator.SetBool("return", false);

                            break;
                        }
                        else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                        {
                            i--; // repeat training trial when error was made

                            // record/update response data
                            thisTrial.response = "double";
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = Time.time - referenceTime;
                            Cue.enabled = false;
                            //ColorCue.SetActive(false);
                            //SquareStim.SetActive(false);
                            MiddleShape.SetActive(false);

                            // Trigger partner finger lift
                            yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                            partnerAnimator.SetInteger("liftFinger", 2);

                            // Display error feedback
                            AlarmRenderer.material.color = AlarmColorOn; // light turns red
                            AlarmLight.enabled = true;
                            Cue.fontSize = PromptFontSize;
                            Cue.text = ErrorMessage;
                            Cue.enabled = true;
                            yield return new WaitForSeconds(ErrorFeedbackDisplay);

                            // Reset animation transitions
                            participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                            partnerAnimator.SetInteger("liftFinger", 0);

                            // Display hold prompt
                            AlarmRenderer.material.color = AlarmColorOff;
                            AlarmLight.enabled = false;
                            Cue.text = HoldPrompt;

                            // Wait for key press and trigger partner finger return animation
                            yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                            yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                            partnerAnimator.SetBool("return", true);

                            // Reset animation transitions
                            yield return null;
                            partnerAnimator.SetBool("return", false);

                            break;
                        }
                        else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                        {
                            thisTrial.response = "none";
                            thisTrial.responseCorrect = false;
                            thisTrial.responseTime = 0;
                        }
                    }
                    // if no response was recorded during response window, display error message
                    if (thisTrial.response == "none")
                    {
                        /* TBD --> should partner respond when participants omit response?
                        // Trigger partner finger lift
                        partnerAnimator.SetInteger("liftFinger", 2);
                        yield return new WaitForSeconds(ErrorFeedbackDelay); // delay slow feedback
                        partnerAnimator.SetInteger("liftFinger", 0); // reset animation transition
                        */
                        i--; // repeat training trial when error was made

                        Cue.fontSize = PromptFontSize;
                        Cue.text = SlowMessage;
                        Cue.enabled = true;
                        //ColorCue.SetActive(false);
                        //SquareStim.SetActive(false);
                        MiddleShape.SetActive(false);
                        yield return new WaitForSeconds(ErrorFeedbackDisplay);

                        // Display hold prompt
                        AlarmRenderer.material.color = AlarmColorOff;
                        AlarmLight.enabled = false;
                        Cue.text = HoldPrompt;

                        yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                        /* TBD --> should partner respond when participants omit response?
                        // Wait for key press and trigger partner finger return animation
                        yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                        yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                        partnerAnimator.SetBool("return", true);

                        // Reset animation transitions
                        yield return null;
                        partnerAnimator.SetBool("return", false);
                        */
                    }
                }
            }
            experimentData.Add(thisTrial);
            yield return new WaitForSeconds(ITI); // ITI
        }
        Instructions.enabled = false;
        yield return new WaitForSeconds(ITI); // ITI
    }

    IEnumerator ImitationInstructions()
    {
        // Activate avatars
        Agents[0].SetActive(true); // Participant Hand
        Agents[1].SetActive(true); // Partner Hand
        Agents[2].SetActive(true); // Avatar1
        Agents[3].SetActive(true); // Avatar2
        LegendHand.enabled = false;
        LegendHandPartner.enabled = false;
        LegendTablet.enabled = false;
        Cue.enabled = false;
        //ColorCue.SetActive(false);
        CircleStim.SetActive(false);
        SquareStim.SetActive(false);

        // Show instructions
        Instructions.text = InstructionTextImitation_I;
        Instructions.enabled = true;
        yield return new WaitUntil(() => Input.GetKey("space"));
        Instructions.enabled = false;
        yield return new WaitForSeconds(0.5f);

        Instructions.text = InstructionTextImitation_II;
        Instructions.enabled = true;
        yield return new WaitUntil(() => Input.GetKey("space"));
        Instructions.enabled = false;
        yield return new WaitForSeconds(0.5f);

        Instructions.text = InstructionTextImitation_III;
        Instructions.enabled = true;
        yield return new WaitUntil(() => Input.GetKey("space"));
        Instructions.enabled = false;
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator TrialProcedure(string BlockCondition, int FamTrialN, int TrialN, int CatchTrialN)
    {
        switch (BlockCondition)
        {
            case "IndInd":

                // Show instructions
                Cue.enabled = false;
                //ColorCue.SetActive(false);
                //CircleStim.SetActive(false);
                //SquareStim.SetActive(false);
                IndexShape.SetActive(false);
                MiddleShape.SetActive(false);
                Instructions.text = "\r\n\r\n<u><b>" + partText + InstructionTextIndInd + ImitatingAvatarInstructionsTextFirstHalf;
                Instructions.enabled = true;
                yield return new WaitUntil(() => Input.GetKey("space"));
                Instructions.enabled = false;
                yield return new WaitForSeconds(0.5f);

                // Init block
                int[] trialListIndIndPractice = trialRandomizer(FamTrialN, 0);
                int[] trialListIndIndTest = testTrialRandomizer(TrialN, CatchTrialN); // randomize trial conditions

                int halfTrialsIndInd = trialListIndIndTest.Length / 2;
                int[] trialListIndIndTestFirstHalf = trialListIndIndTest.Take(halfTrialsIndInd).ToArray();
                int[] trialListIndIndTestSecondHalf = trialListIndIndTest.Skip(halfTrialsIndInd).ToArray();

                int[] trialListIndIndFirstHalf = ConcatenateTrials(trialListIndIndPractice, trialListIndIndTestFirstHalf);
                int[] trialListIndIndSecondHalf = ConcatenateTrials(trialListIndIndPractice, trialListIndIndTestSecondHalf);

                int[] trialListIndInd = ConcatenateTrials(trialListIndIndFirstHalf, trialListIndIndSecondHalf);

                string arrayAsString = string.Join(", ", trialListIndInd);
                Debug.Log(arrayAsString);

                int[] imitatingAvatarListIndInd = GenerateAvatarList(trialListIndInd, firstImitator); // generate list containing information about which avatar imitates response --> see function below 
                string tempString = string.Join(", ", imitatingAvatarListIndInd);
                Debug.Log(tempString);

                for (int i = 0; i < (trialListIndInd.Length); i++) // loop over # of trials
                {
                    // init TrialData class to save trial data
                    TrialData thisTrial = new TrialData();
                    thisTrial.ParticipantID = ParticipantID;
                    thisTrial.HandPosition = HandPosition;
                    thisTrial.blockCounter = blockCounter;
                    thisTrial.blockCondition = blockCondition;
                    thisTrial.blockOrder = blockOrder;
                    thisTrial.SRMapping = SRMapping;
                    thisTrial.firstImitator = firstImitator;
                    thisTrial.imitationDelayAvatar1 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.returnDelayAvatar1 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    if (imitatingAvatarListIndInd[i] == 1)
                    {
                        thisTrial.imitatingAvatar = "right";
                    }
                    else if (imitatingAvatarListIndInd[i] == 2)
                    {
                        thisTrial.imitatingAvatar = "left";
                    }

                    // Init trial variables
                    thisTrial.trialCounter = i + 1;
                    Debug.Log("Trial #: " + thisTrial.trialCounter);
                    thisTrial.response = null;
                    thisTrial.responseTime = 0;
                    thisTrial.responseCorrect = false; 
  
                    // Display hold prompt and wait until both keys are pressed
                    Cue.text = HoldPrompt;
                    Cue.fontSize = PromptFontSize;
                    Cue.enabled = true;
                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                    // Display fixation cross after delay and listen for key release before cue onset
                    referenceTime = Time.time;
                    while ((Time.time-referenceTime) < (FixationCrossDelay+FixationCrossDisplay)) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
                    {
                        yield return null; 
                        if (!Input.GetKey(IndexKey) || !Input.GetKey(MiddleKey))
                        {
                            thisTrial.response = "early";
                            break;
                        }
                        if (Time.time-referenceTime >= FixationCrossDelay)
                        {
                            Cue.fontSize = CueFontSize;
                            Cue.text = "+";
                        }
                    }

                    // If response was made before cue presentation --> skip/repeat trial
                    if (thisTrial.response == "early") 
                    {
                        i--; // repeat trial --> double check if this works as intended!
                        Cue.fontSize = PromptFontSize;
                        Cue.text = EarlyMessage;
                        Cue.enabled = true;
                        yield return new WaitForSeconds(ErrorFeedbackDisplay);

                        // Display hold prompt
                        Cue.text = HoldPrompt;
                        yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                    }
                    // If no response was made before cue presentation --> proceed with trial structure 
                    else if (thisTrial.response == null)
                    {
                        // Case: Cue = 1
                        if (trialListIndInd[i] == 1) // check trial conditions
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_1;
                            //ColorCue.SetActive(true);
                            //CircleStim.SetActive(true);
                            IndexShape.SetActive(true);
                            Cue.text = IndexCue;
                            thisTrial.trialCondition = 1;

                            // set reference time
                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift

                                    // Trigger THEY finger lift
                                    if (imitatingAvatarListIndInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1); // Imitation delay
                                        Avatar1Animator.SetInteger("liftFinger", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        Avatar1Animator.SetInteger("liftFinger", 0);

                                        // Display hold prompt
                                        Cue.fontSize = PromptFontSize;
                                        Cue.text = HoldPrompt;
                                        Cue.enabled = true;

                                        // Wait for key press and trigger WE finger return animation
                                        yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                        participantAnimator.SetBool("return", true);

                                        // Trigger THEY finger return animation
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar1Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        Avatar1Animator.SetBool("return", false);
                                    }
                                    else if (imitatingAvatarListIndInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1); // Imitation delay
                                        Avatar2Animator.SetInteger("liftFinger", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        Avatar2Animator.SetInteger("liftFinger", 0);

                                        // Display hold prompt
                                        Cue.fontSize = PromptFontSize;
                                        Cue.text = HoldPrompt;
                                        Cue.enabled = true;

                                        // Wait for key press and trigger WE finger return animation
                                        yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                        participantAnimator.SetBool("return", true);

                                        // Trigger THEY finger return animation
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        Avatar2Animator.SetBool("return", false);
                                    }

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // wrong response
                                {
                                    if(thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                    {
                                        i--; // repeat training trial if error was made
                                    }
                                   
                                    // record/update response data
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift
                                    yield return new WaitForSeconds(ErrorFeedbackDelay); // delay error feedback

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            // if no response was recorded during response window, display error message
                            if (thisTrial.response == "none") 
                            {
                                if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                {
                                    i--; // repeat training trial if no response was made
                                }

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //CircleStim.SetActive(false);
                                IndexShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                            }
                        }
                        // Case: Cue = 2
                        else if (trialListIndInd[i] == 2) // check trial condition
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_2;
                            //ColorCue.SetActive(true);
                            //SquareStim.SetActive(true);
                            MiddleShape.SetActive(true);
                            Cue.text = MiddleCue;
                            thisTrial.trialCondition = 2;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
                            {
                                yield return null;
                                if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                                {
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2);

                                    // Trigger THEY finger lift
                                    if (imitatingAvatarListIndInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1);
                                        Avatar1Animator.SetInteger("liftFinger", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration outcome display

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        Avatar1Animator.SetInteger("liftFinger", 0);

                                        // Display hold prompt
                                        Cue.fontSize = PromptFontSize;
                                        Cue.text = HoldPrompt;
                                        Cue.enabled = true;

                                        // Wait for key press and trigger WE finger return animation
                                        yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                        participantAnimator.SetBool("return", true);

                                        // Trigger THEY finger return animation
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar1Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        Avatar1Animator.SetBool("return", false);
                                    }
                                    else if (imitatingAvatarListIndInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1);
                                        Avatar2Animator.SetInteger("liftFinger", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration outcome display

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        Avatar2Animator.SetInteger("liftFinger", 0);

                                        // Display hold prompt
                                        Cue.fontSize = PromptFontSize;
                                        Cue.text = HoldPrompt;
                                        Cue.enabled = true;

                                        // Wait for key press and trigger WE finger return animation
                                        yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                        participantAnimator.SetBool("return", true);

                                        // Trigger THEY finger return animation
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        Avatar2Animator.SetBool("return", false);
                                    }

                                    break;
                                }
                                else if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey))
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1);
                                    yield return new WaitForSeconds(ErrorFeedbackDelay);

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0);
                                    AlarmRenderer.material.color = AlarmColorOn;
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey))
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }

                            if (thisTrial.response == "none")
                            {
                                if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                {
                                    i--; // repeat training trial if no response was made
                                }

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //SquareStim.SetActive(false);
                                MiddleShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                            }
                        }
                        // catch trial 3
                        else if (trialListIndInd[i] == 3)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_1;
                            //ColorCue.SetActive(true);
                            //CircleStim.SetActive(true);
                            IndexShape.SetActive(true);
                            Cue.text = IndexCue;
                            thisTrial.trialCondition = 3;

                            // set reference time
                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift

                                    // Trigger THEY finger lift
                                    if (imitatingAvatarListIndInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1); // Imitation delay
                                        Avatar1Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        Avatar1Animator.SetInteger("catch", 0);
                                    }
                                    else if (imitatingAvatarListIndInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1); // Imitation delay
                                        Avatar2Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        Avatar2Animator.SetInteger("catch", 0);
                                    }

                                    // Wait for catch trial response
                                    referenceTime = Time.time;
                                    while ((Time.time - referenceTime) <= ResponseWindowCatch)
                                    {
                                        yield return null;
                                        if (!Input.GetKey(MiddleKey))
                                        {
                                            thisTrial.catchTrialResponse = true;
                                            yield return new WaitForSeconds(ResponseWindowCatch - (Time.time - referenceTime));
                                            break;
                                        }
                                        else if (Input.GetKey(MiddleKey))
                                        {
                                            thisTrial.catchTrialResponse = false;
                                        }
                                    }

                                    if (thisTrial.catchTrialResponse == false)
                                    {
                                        Cue.fontSize = CatchPromptFontSize;
                                        Cue.text = CatchPrompt;
                                        Cue.enabled = true;
                                        yield return new WaitForSeconds(CatchFeedbackDisplay);
                                    }

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (imitatingAvatarListIndInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar1Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        Avatar1Animator.SetBool("return", false);
                                    }
                                    else if (imitatingAvatarListIndInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        Avatar2Animator.SetBool("return", false);
                                    }

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // wrong response
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift
                                    yield return new WaitForSeconds(ErrorFeedbackDelay); // delay error feedback

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            if (thisTrial.response == "none")
                            {
                                i--; // repeat trial --> double check if this works as intended!
                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //CircleStim.SetActive(false);
                                IndexShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                            }
                        }
                        // catch trial 4
                        else if (trialListIndInd[i] == 4)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_2;
                            //ColorCue.SetActive(true);
                            //SquareStim.SetActive(true);
                            MiddleShape.SetActive(true);
                            Cue.text = MiddleCue;
                            thisTrial.trialCondition = 4;

                            // set reference time
                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 1 --> index finger lift

                                    // Trigger catch trial animation
                                    if (imitatingAvatarListIndInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1); // Imitation delay
                                        Avatar1Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        Avatar1Animator.SetInteger("catch", 0);
                                    }
                                    else if (imitatingAvatarListIndInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1); // Imitation delay
                                        Avatar2Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        Avatar2Animator.SetInteger("catch", 0);
                                    }

                                    // Wait for catch trial response
                                    referenceTime = Time.time;
                                    while ((Time.time - referenceTime) <= ResponseWindowCatch)
                                    {
                                        yield return null;
                                        if (!Input.GetKey(IndexKey))
                                        {
                                            thisTrial.catchTrialResponse = true;
                                            yield return new WaitForSeconds(ResponseWindowCatch - (Time.time - referenceTime));
                                            break;
                                        }
                                        else if (Input.GetKey(IndexKey))
                                        {
                                            thisTrial.catchTrialResponse = false;
                                        }
                                    }

                                    if (thisTrial.catchTrialResponse == false)
                                    {
                                        Cue.fontSize = CatchPromptFontSize;
                                        Cue.text = CatchPrompt;
                                        Cue.enabled = true;
                                        yield return new WaitForSeconds(CatchFeedbackDisplay);
                                    }

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (imitatingAvatarListIndInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar1Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        Avatar1Animator.SetBool("return", false);
                                    }
                                    else if (imitatingAvatarListIndInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        Avatar2Animator.SetBool("return", false);
                                    }

                                    break;
                                }
                                else if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // wrong response
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 
                                    yield return new WaitForSeconds(ErrorFeedbackDelay); // delay error feedback

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            if (thisTrial.response == "none")
                            {
                                i--; // repeat trial --> double check if this works as intended!
                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //SquareStim.SetActive(false);
                                MiddleShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                            }
                        }
                    }
                    Debug.Log(thisTrial.ToString());
                    experimentData.Add(thisTrial);
                    yield return new WaitForSeconds(ITI); // ITI
                    if (i == FamTrialN-1 && (thisTrial.trialCondition == 1 || thisTrial.trialCondition == 2))
                    {
                        // Show instructions again after training
                        Cue.enabled = false;
                        //ColorCue.SetActive(false);
                        //CircleStim.SetActive(false);
                        //SquareStim.SetActive(false);
                        IndexShape.SetActive(false);
                        MiddleShape.SetActive(false);
                        Instructions.text = InstructionTextTest;
                        Instructions.enabled = true;
                        yield return new WaitUntil(() => Input.GetKey("space"));
                        Instructions.enabled = false;
                        yield return new WaitForSeconds(0.5f);
                    }
                    if (thisTrial.trialCounter == trialListIndInd.Length/2 && (thisTrial.trialCondition == 1 || thisTrial.trialCondition == 2))
                    {
                        // Show instructions about imitating avatar change after first half of block
                        Cue.enabled = false;
                        IndexShape.SetActive(false);
                        MiddleShape.SetActive(false);
                        Instructions.text = ImitatingAvatarInstructionsTextSecondHalf;
                        Instructions.enabled = true;
                        yield return new WaitUntil(() => Input.GetKey("space"));
                        Instructions.enabled = false;
                        yield return new WaitForSeconds(0.5f);
                    }
                    if (i == (trialListIndInd.Length/2+FamTrialN)-1 && (thisTrial.trialCondition == 1 || thisTrial.trialCondition == 2))
                    {
                        // Show instructions again after training
                        Cue.enabled = false;
                        //ColorCue.SetActive(false);
                        //CircleStim.SetActive(false);
                        //SquareStim.SetActive(false);
                        IndexShape.SetActive(false);
                        MiddleShape.SetActive(false);
                        Instructions.text = InstructionTextTest;
                        Instructions.enabled = true;
                        yield return new WaitUntil(() => Input.GetKey("space"));
                        Instructions.enabled = false;
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                break;

            case "IndGroup":

                // Show instructions
                Cue.enabled = false;
                //ColorCue.SetActive(false);
                //CircleStim.SetActive(false);
                //SquareStim.SetActive(false);
                IndexShape.SetActive(false);
                MiddleShape.SetActive(false);
                Instructions.text = "\r\n\r\n<u><b>" + partText + InstructionTextIndGroup;
                Instructions.enabled = true;
                yield return new WaitUntil(() => Input.GetKey("space"));
                Instructions.enabled = false;
                yield return new WaitForSeconds(0.5f);

                // Init block
                int[] trialListIndGroupPractice = trialRandomizer(FamTrialN, 0);
                int[] trialListIndGroupTest = testTrialRandomizer(TrialN, CatchTrialN); // randomize trial conditions

                int halfTrialsIndGroup = trialListIndGroupTest.Length / 2;
                int[] trialListIndGroupTestFirstHalf = trialListIndGroupTest.Take(halfTrialsIndGroup).ToArray();
                int[] trialListIndGroupTestSecondHalf = trialListIndGroupTest.Skip(halfTrialsIndGroup).ToArray();

                int[] trialListIndGroupFirstHalf = ConcatenateTrials(trialListIndGroupPractice, trialListIndGroupTestFirstHalf);
                int[] trialListIndGroupSecondHalf = ConcatenateTrials(trialListIndGroupPractice, trialListIndGroupTestSecondHalf);

                int[] trialListIndGroup = ConcatenateTrials(trialListIndGroupFirstHalf, trialListIndGroupSecondHalf);

                arrayAsString = string.Join(", ", trialListIndGroup);
                Debug.Log(arrayAsString);

                for (int i = 0; i < (trialListIndGroup.Length); i++) // loop over # of trials
                {
                    // init TrialData class to save trial data
                    TrialData thisTrial = new TrialData();
                    thisTrial.ParticipantID = ParticipantID;
                    thisTrial.HandPosition = HandPosition;
                    thisTrial.blockCounter = blockCounter;
                    thisTrial.blockCondition = blockCondition;
                    thisTrial.blockOrder = blockOrder;
                    thisTrial.SRMapping = SRMapping;
                    thisTrial.imitationDelayAvatar1 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.imitationDelayAvatar2 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.returnDelayAvatar1 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.returnDelayAvatar2 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.imitatingAvatar = "both";

                    // Init trial variables
                    thisTrial.trialCounter = i + 1;
                    Debug.Log("Trial #: " + thisTrial.trialCounter);
                    thisTrial.response = null;
                    thisTrial.responseTime = 0;
                    thisTrial.responseCorrect = false;

                    // Display hold prompt and wait until both keys are pressed
                    Cue.text = HoldPrompt;
                    Cue.fontSize = PromptFontSize;
                    Cue.enabled = true;
                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                    // Display fixation cross after delay and listen for key release before cue onset
                    referenceTime = Time.time;
                    while ((Time.time - referenceTime) < (FixationCrossDelay + FixationCrossDisplay)) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
                    {
                        yield return null;
                        if (!Input.GetKey(IndexKey) || !Input.GetKey(MiddleKey))
                        {
                            thisTrial.response = "early";
                            break;
                        }
                        if (Time.time - referenceTime >= FixationCrossDelay)
                        {
                            Cue.fontSize = CueFontSize;
                            Cue.text = "+";
                        }
                    }

                    // If response was made before cue presentation --> skip/repeat trial
                    if (thisTrial.response == "early")
                    {
                        i--; // repeat trial --> double check if this works as intended!
                        Cue.fontSize = PromptFontSize;
                        Cue.text = EarlyMessage;
                        Cue.enabled = true;
                        yield return new WaitForSeconds(ErrorFeedbackDisplay);

                        // Display hold prompt
                        Cue.text = HoldPrompt;
                        yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                    }
                    // If no response was made before cue presentation --> proceed with trial structure
                    else if (thisTrial.response == null)
                    {
                        if (trialListIndGroup[i] == 1)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_1;
                            //ColorCue.SetActive(true);
                            //CircleStim.SetActive(true);
                            IndexShape.SetActive(true);
                            Cue.text = IndexCue;
                            thisTrial.trialCondition = 1;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
                            {
                                yield return null;
                                if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey))
                                {
                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift

                                    // Trigger THEY finger lift
                                    if (thisTrial.imitationDelayAvatar1 <= thisTrial.imitationDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1); // Imitation delay
                                        Avatar1Animator.SetInteger("liftFinger", 1);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.imitationDelayAvatar1);
                                        Avatar2Animator.SetInteger("liftFinger", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }
                                    else if (thisTrial.imitationDelayAvatar2 <= thisTrial.imitationDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2); // Imitation delay
                                        Avatar2Animator.SetInteger("liftFinger", 1);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.imitationDelayAvatar2);
                                        Avatar1Animator.SetInteger("liftFinger", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0);
                                    Avatar1Animator.SetInteger("liftFinger", 0);
                                    Avatar2Animator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (thisTrial.returnDelayAvatar1 <= thisTrial.returnDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar1Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);
                                    }
                                    else if (thisTrial.returnDelayAvatar2 <= thisTrial.returnDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2);
                                        Avatar2Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayAvatar2);
                                        Avatar1Animator.SetBool("return", true);
                                    }

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    Avatar1Animator.SetBool("return", false);
                                    Avatar2Animator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift
                                    yield return new WaitForSeconds(ErrorFeedbackDelay); // delay error feedback

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey))
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            if (thisTrial.response == "none")
                            {
                                if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                {
                                    i--; // repeat training trial if no response was made
                                }

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //CircleStim.SetActive(false);
                                IndexShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                            }

                        }
                        // Case: Cue = 2
                        else if (trialListIndGroup[i] == 2) // check trial condition
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_2;
                            //ColorCue.SetActive(true);
                            //SquareStim.SetActive(true);
                            MiddleShape.SetActive(true);
                            Cue.text = MiddleCue;
                            thisTrial.trialCondition = 2;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
                            {
                                yield return null;
                                if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                                {
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2);

                                    // Trigger THEY finger lift
                                    if (thisTrial.imitationDelayAvatar1 <= thisTrial.imitationDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1); // Imitation delay
                                        Avatar1Animator.SetInteger("liftFinger", 2);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.imitationDelayAvatar1);
                                        Avatar2Animator.SetInteger("liftFinger", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }
                                    else if (thisTrial.imitationDelayAvatar2 <= thisTrial.imitationDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2); // Imitation delay
                                        Avatar2Animator.SetInteger("liftFinger", 2);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.imitationDelayAvatar2);
                                        Avatar1Animator.SetInteger("liftFinger", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0);
                                    Avatar1Animator.SetInteger("liftFinger", 0);
                                    Avatar2Animator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (thisTrial.returnDelayAvatar1 <= thisTrial.returnDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar1Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);
                                    }
                                    else if (thisTrial.returnDelayAvatar2 <= thisTrial.returnDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2);
                                        Avatar2Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayAvatar2);
                                        Avatar1Animator.SetBool("return", true);
                                    }

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    Avatar1Animator.SetBool("return", false);
                                    Avatar2Animator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey))
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1);
                                    yield return new WaitForSeconds(ErrorFeedbackDelay); // delay error feedback

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey))
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            if (thisTrial.response == "none")
                            {
                                if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                {
                                    i--; // repeat training trial if no response was made
                                }

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //SquareStim.SetActive(false);
                                MiddleShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                            }
                        }
                        // catch trial 3
                        else if (trialListIndGroup[i] == 3)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_1;
                            //ColorCue.SetActive(true);
                            //CircleStim.SetActive(true);
                            IndexShape.SetActive(true);
                            Cue.text = IndexCue;
                            thisTrial.trialCondition = 3;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
                            {
                                yield return null;
                                if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey))
                                {
                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift

                                    // Trigger THEY finger lift
                                    if (thisTrial.imitationDelayAvatar1 <= thisTrial.imitationDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1); // Imitation delay
                                        Avatar1Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.imitationDelayAvatar1);
                                        Avatar2Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }
                                    else if (thisTrial.imitationDelayAvatar2 <= thisTrial.imitationDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2); // Imitation delay
                                        Avatar2Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.imitationDelayAvatar2);
                                        Avatar1Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }

                                    // Wait for catch trial response
                                    referenceTime = Time.time;
                                    while ((Time.time - referenceTime) <= ResponseWindowCatch)
                                    {
                                        yield return null;
                                        if (!Input.GetKey(MiddleKey))
                                        {
                                            thisTrial.catchTrialResponse = true;
                                            yield return new WaitForSeconds(ResponseWindowCatch - (Time.time - referenceTime));
                                            break;
                                        }
                                        else if (Input.GetKey(MiddleKey))
                                        {
                                            thisTrial.catchTrialResponse = false;
                                        }
                                    }

                                    if (thisTrial.catchTrialResponse == false)
                                    {
                                        Cue.fontSize = CatchPromptFontSize;
                                        Cue.text = CatchPrompt;
                                        Cue.enabled = true;
                                        yield return new WaitForSeconds(CatchFeedbackDisplay);
                                    }

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0);
                                    Avatar1Animator.SetInteger("catch", 0);
                                    Avatar2Animator.SetInteger("catch", 0);

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (thisTrial.returnDelayAvatar1 <= thisTrial.returnDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar1Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);
                                    }
                                    else if (thisTrial.returnDelayAvatar2 <= thisTrial.returnDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2);
                                        Avatar2Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayAvatar2);
                                        Avatar1Animator.SetBool("return", true);
                                    }

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    Avatar1Animator.SetBool("return", false);
                                    Avatar2Animator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift
                                    yield return new WaitForSeconds(ErrorFeedbackDelay); // delay error feedback

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey))
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            if (thisTrial.response == "none")
                            {
                                i--; // repeat trial --> double check if this works as intended!
                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //CircleStim.SetActive(false);
                                IndexShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                            }
                        }
                        // catch trial 4
                        else if (trialListIndGroup[i] == 4)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_2;
                            //ColorCue.SetActive(true);
                            //SquareStim.SetActive(true);
                            MiddleShape.SetActive(true);
                            Cue.text = MiddleCue;
                            thisTrial.trialCondition = 4;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
                            {
                                yield return null;
                                if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                                {
                                    // record/update response data
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 1 --> index finger lift

                                    // Trigger THEY finger lift
                                    if (thisTrial.imitationDelayAvatar1 <= thisTrial.imitationDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1); // Imitation delay
                                        Avatar1Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.imitationDelayAvatar1);
                                        Avatar2Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }
                                    else if (thisTrial.imitationDelayAvatar2 <= thisTrial.imitationDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2); // Imitation delay
                                        Avatar2Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.imitationDelayAvatar2);
                                        Avatar1Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }

                                    // Wait for catch trial response
                                    referenceTime = Time.time;
                                    while ((Time.time - referenceTime) <= ResponseWindowCatch)
                                    {
                                        yield return null;
                                        if (!Input.GetKey(IndexKey))
                                        {
                                            thisTrial.catchTrialResponse = true;
                                            yield return new WaitForSeconds(ResponseWindowCatch - (Time.time - referenceTime));
                                            break;
                                        }
                                        else if (Input.GetKey(IndexKey))
                                        {
                                            thisTrial.catchTrialResponse = false;
                                        }
                                    }

                                    if (thisTrial.catchTrialResponse == false)
                                    {
                                        Cue.fontSize = CatchPromptFontSize;
                                        Cue.text = CatchPrompt;
                                        Cue.enabled = true;
                                        yield return new WaitForSeconds(CatchFeedbackDisplay);
                                    }

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0);
                                    Avatar1Animator.SetInteger("catch", 0);
                                    Avatar2Animator.SetInteger("catch", 0);

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (thisTrial.returnDelayAvatar1 <= thisTrial.returnDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1);
                                        Avatar1Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);
                                    }
                                    else if (thisTrial.returnDelayAvatar2 <= thisTrial.returnDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2);
                                        Avatar2Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayAvatar2);
                                        Avatar1Animator.SetBool("return", true);
                                    }

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    Avatar1Animator.SetBool("return", false);
                                    Avatar2Animator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey))
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 2 --> middle finger lift
                                    yield return new WaitForSeconds(ErrorFeedbackDelay); // delay error feedback

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey))
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Display error feedback
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey))
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            if (thisTrial.response == "none")
                            {
                                i--; // repeat trial --> double check if this works as intended!
                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //SquareStim.SetActive(false);
                                MiddleShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                            }
                        }
                    }
                    Debug.Log(thisTrial.ToString());
                    experimentData.Add(thisTrial);
                    yield return new WaitForSeconds(ITI); // ITI
                    if (i == FamTrialN-1 && (thisTrial.trialCondition == 1 || thisTrial.trialCondition == 2))
                    {
                        // Show instructions again after training
                        Cue.enabled = false;
                        //ColorCue.SetActive(false);
                        //CircleStim.SetActive(false);
                        //SquareStim.SetActive(false);
                        IndexShape.SetActive(false);
                        MiddleShape.SetActive(false);
                        Instructions.text = InstructionTextTest;
                        Instructions.enabled = true;
                        yield return new WaitUntil(() => Input.GetKey("space"));
                        Instructions.enabled = false;
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                break;

            case "GroupInd":

                // Show instructions
                Cue.enabled = false;
                //ColorCue.SetActive(false);
                //CircleStim.SetActive(false);
                //SquareStim.SetActive(false);
                IndexShape.SetActive(false);
                MiddleShape.SetActive(false);
                Instructions.text = "\r\n\r\n<u><b>" + partText + InstructionTextGroupInd + ImitatingAvatarInstructionsTextFirstHalf;
                Instructions.enabled = true;
                yield return new WaitUntil(() => Input.GetKey("space"));
                Instructions.enabled = false;
                yield return new WaitForSeconds(0.5f);

                // Init block
                int[] trialListGroupIndPractice = trialRandomizer(FamTrialN, 0);
                int[] trialListGroupIndTest = testTrialRandomizer(TrialN, CatchTrialN); // randomize trial conditions

                int halfTrialsGroupInt = trialListGroupIndTest.Length / 2;
                int[] trialListGroupIndTestFirstHalf = trialListGroupIndTest.Take(halfTrialsGroupInt).ToArray();
                int[] trialListGroupIndTestSecondHalf = trialListGroupIndTest.Skip(halfTrialsGroupInt).ToArray();

                int[] trialListGroupIndFirstHalf = ConcatenateTrials(trialListGroupIndPractice, trialListGroupIndTestFirstHalf);
                int[] trialListGroupIndSecondHalf = ConcatenateTrials(trialListGroupIndPractice, trialListGroupIndTestSecondHalf);

                int[] trialListGroupInd = ConcatenateTrials(trialListGroupIndFirstHalf, trialListGroupIndSecondHalf);

                int[] imitatingAvatarListGroupInd = GenerateAvatarList(trialListGroupInd, firstImitator); // generate list containing information about which avatar imitates response --> see function below 

                arrayAsString = string.Join(", ", trialListGroupInd);
                Debug.Log(arrayAsString);

                tempString = string.Join(", ", imitatingAvatarListGroupInd);
                Debug.Log(tempString);

                for (int i = 0; i < (trialListGroupInd.Length); i++) // loop over # of trials
                {
                    // init TrialData class to save trial data
                    TrialData thisTrial = new TrialData();
                    thisTrial.ParticipantID = ParticipantID;
                    thisTrial.HandPosition = HandPosition;
                    thisTrial.blockCounter = blockCounter;
                    thisTrial.blockCondition = blockCondition;
                    thisTrial.blockOrder = blockOrder;
                    thisTrial.SRMapping = SRMapping;
                    thisTrial.firstImitator = firstImitator;
                    thisTrial.imitationDelayAvatar1 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.returnDelayAvatar1 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.responseDelayPartner = UnityEngine.Random.Range(PartnerResponseDelayMin, PartnerResponseDelayMax);
                    thisTrial.returnDelayPartner = UnityEngine.Random.Range(PartnerResponseDelayMin, PartnerResponseDelayMax);
                    if (imitatingAvatarListGroupInd[i] == 1)
                    {
                        thisTrial.imitatingAvatar = "right";
                    }
                    else if (imitatingAvatarListGroupInd[i] == 2)
                    {
                        thisTrial.imitatingAvatar = "left";
                    }

                    // Init trial variables
                    thisTrial.trialCounter = i + 1;
                    Debug.Log("Trial #: " + thisTrial.trialCounter);
                    thisTrial.response = null;
                    thisTrial.responseTime = 0;
                    thisTrial.responseCorrect = false;

                    // Display hold prompt and wait until both keys are pressed
                    Cue.text = HoldPrompt;
                    Cue.fontSize = PromptFontSize;
                    Cue.enabled = true;
                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                    // Display fixation cross after delay and listen for key release before cue onset
                    referenceTime = Time.time;
                    while ((Time.time - referenceTime) < (FixationCrossDelay + FixationCrossDisplay)) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
                    {
                        yield return null;
                        if (!Input.GetKey(IndexKey) || !Input.GetKey(MiddleKey))
                        {
                            thisTrial.response = "early";
                            break;
                        }
                        if (Time.time - referenceTime >= FixationCrossDelay)
                        {
                            Cue.fontSize = CueFontSize;
                            Cue.text = "+";
                        }
                    }

                    // If response was made before cue presentation --> skip/repeat trial
                    if (thisTrial.response == "early")
                    {
                        i--; // repeat trial --> double check if this works as intended!
                        Cue.fontSize = PromptFontSize;
                        Cue.text = EarlyMessage;
                        Cue.enabled = true;
                        yield return new WaitForSeconds(ErrorFeedbackDisplay);

                        // Display hold prompt
                        Cue.text = HoldPrompt;
                        yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                    }
                    else if (thisTrial.response == null)
                    {
                        if (trialListGroupInd[i] == 1)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_1;
                            //ColorCue.SetActive(true);
                            //CircleStim.SetActive(true);
                            IndexShape.SetActive(true);
                            Cue.text = IndexCue;
                            thisTrial.trialCondition = 1;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger participant finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift

                                    // Trigger partner finger lift after variable delay
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Trigger THEY finger lift
                                    if (imitatingAvatarListGroupInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar1Animator.SetInteger("liftFinger", 1);

                                        // Show outcome
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        partnerAnimator.SetInteger("liftFinger", 0);
                                        Avatar1Animator.SetInteger("liftFinger", 0);

                                        // Display hold prompt
                                        Cue.fontSize = PromptFontSize;
                                        Cue.text = HoldPrompt;
                                        Cue.enabled = true;

                                        // Wait for key press and trigger WE finger return animation
                                        yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                        participantAnimator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                        partnerAnimator.SetBool("return", true);

                                        // Trigger THEY finger return animation
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar1Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        partnerAnimator.SetBool("return", false);
                                        Avatar1Animator.SetBool("return", false);
                                    }
                                    else if (imitatingAvatarListGroupInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar2Animator.SetInteger("liftFinger", 1);

                                        // Show outcome
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        partnerAnimator.SetInteger("liftFinger", 0);
                                        Avatar2Animator.SetInteger("liftFinger", 0);

                                        // Display hold prompt
                                        Cue.fontSize = PromptFontSize;
                                        Cue.text = HoldPrompt;
                                        Cue.enabled = true;

                                        // Wait for key press and trigger WE finger return animation
                                        yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                        participantAnimator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                        partnerAnimator.SetBool("return", true);

                                        // Trigger THEY finger return animation
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar2Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        partnerAnimator.SetBool("return", false);
                                        Avatar2Animator.SetBool("return", false);
                                    }

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // wrong response
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger partner finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger partner finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            // if no response was recorded during response window, display error message
                            if (thisTrial.response == "none")
                            {
                                /* TBD --> should partner respond when participants omit response?
                                // Trigger partner finger lift
                                partnerAnimator.SetInteger("liftFinger", 1);
                                yield return new WaitForSeconds(ErrorFeedbackDelay); // delay slow feedback
                                partnerAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                */
                                if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                {
                                    i--; // repeat training trial if no response was made
                                }

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //CircleStim.SetActive(false);
                                IndexShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                /* TBD --> should partner respond when participants omit response?
                                // Wait for key press and trigger partner finger return animation
                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                partnerAnimator.SetBool("return", true);

                                // Reset animation transitions
                                yield return null;
                                partnerAnimator.SetBool("return", false);
                                */
                            }
                        }
                        else if (trialListGroupInd[i] == 2)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_2;
                            //ColorCue.SetActive(true);
                            //SquareStim.SetActive(true);
                            MiddleShape.SetActive(true);
                            Cue.text = MiddleCue;
                            thisTrial.trialCondition = 2;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger participant finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift

                                    // Trigger partner finger lift after variable delay
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Trigger THEY finger lift
                                    if (imitatingAvatarListGroupInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar1Animator.SetInteger("liftFinger", 2);

                                        // Show outcome
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        partnerAnimator.SetInteger("liftFinger", 0);
                                        Avatar1Animator.SetInteger("liftFinger", 0);

                                        // Display hold prompt
                                        Cue.fontSize = PromptFontSize;
                                        Cue.text = HoldPrompt;
                                        Cue.enabled = true;

                                        // Wait for key press and trigger WE finger return animation
                                        yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                        participantAnimator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                        partnerAnimator.SetBool("return", true);

                                        // Trigger THEY finger return animation
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar1Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        partnerAnimator.SetBool("return", false);
                                        Avatar1Animator.SetBool("return", false);
                                    }
                                    else if (imitatingAvatarListGroupInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar2Animator.SetInteger("liftFinger", 2);

                                        // Show outcome
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        partnerAnimator.SetInteger("liftFinger", 0);
                                        Avatar2Animator.SetInteger("liftFinger", 0);

                                        // Display hold prompt
                                        Cue.fontSize = PromptFontSize;
                                        Cue.text = HoldPrompt;
                                        Cue.enabled = true;

                                        // Wait for key press and trigger WE finger return animation
                                        yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                        participantAnimator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                        partnerAnimator.SetBool("return", true);

                                        // Trigger THEY finger return animation
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar2Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        partnerAnimator.SetBool("return", false);
                                        Avatar2Animator.SetBool("return", false);
                                    }

                                    break;
                                }
                                else if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // wrong response
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger partner finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger partner finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            // if no response was recorded during response window, display error message
                            if (thisTrial.response == "none")
                            {
                                /* TBD --> should partner respond when participants omit response?
                                // Trigger partner finger lift
                                partnerAnimator.SetInteger("liftFinger", 1);
                                yield return new WaitForSeconds(ErrorFeedbackDelay); // delay slow feedback
                                partnerAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                */
                                if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2 || thisTrial.trialCounter == 23 || thisTrial.trialCounter == 24)
                                {
                                    i--; // repeat training trial if error was made
                                }

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //SquareStim.SetActive(false);
                                MiddleShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                /* TBD --> should partner respond when participants omit response?
                                // Wait for key press and trigger partner finger return animation
                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                partnerAnimator.SetBool("return", true);

                                // Reset animation transitions
                                yield return null;
                                partnerAnimator.SetBool("return", false);
                                */
                            }

                        }
                        // catch trial 3
                        else if (trialListGroupInd[i] == 3)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_1;
                            //ColorCue.SetActive(true);
                            //CircleStim.SetActive(true);
                            IndexShape.SetActive(true);
                            Cue.text = IndexCue;
                            thisTrial.trialCondition = 3;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger participant finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift

                                    // Trigger partner finger lift after variable delay
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Trigger THEY finger lift
                                    if (imitatingAvatarListGroupInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar1Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        partnerAnimator.SetInteger("liftFinger", 0);
                                        Avatar1Animator.SetInteger("catch", 0);
                                    }
                                    else if (imitatingAvatarListGroupInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar2Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        partnerAnimator.SetInteger("liftFinger", 0);
                                        Avatar2Animator.SetInteger("catch", 0);
                                    }

                                    // Wait for catch trial response
                                    referenceTime = Time.time;
                                    while ((Time.time - referenceTime) <= ResponseWindowCatch)
                                    {
                                        yield return null;
                                        if (!Input.GetKey(MiddleKey))
                                        {
                                            thisTrial.catchTrialResponse = true;
                                            yield return new WaitForSeconds(ResponseWindowCatch - (Time.time - referenceTime));
                                            break;
                                        }
                                        else if (Input.GetKey(MiddleKey))
                                        {
                                            thisTrial.catchTrialResponse = false;
                                        }
                                    }

                                    if (thisTrial.catchTrialResponse == false)
                                    {
                                        Cue.fontSize = CatchPromptFontSize;
                                        Cue.text = CatchPrompt;
                                        Cue.enabled = true;
                                        yield return new WaitForSeconds(CatchFeedbackDisplay);
                                    }

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (imitatingAvatarListGroupInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar1Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        partnerAnimator.SetBool("return", false);
                                        Avatar1Animator.SetBool("return", false);
                                    }
                                    else if (imitatingAvatarListGroupInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar2Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        partnerAnimator.SetBool("return", false);
                                        Avatar2Animator.SetBool("return", false);
                                    }

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // wrong response
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger partner finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger partner finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            // if no response was recorded during response window, display error message
                            if (thisTrial.response == "none")
                            {
                                i--; // repeat trial --> double check if this works as intended!

                                /* TBD --> should partner respond when participants omit response?
                                // Trigger partner finger lift
                                partnerAnimator.SetInteger("liftFinger", 1);
                                yield return new WaitForSeconds(ErrorFeedbackDelay); // delay slow feedback
                                partnerAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                */

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //CircleStim.SetActive(false);
                                IndexShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                /* TBD --> should partner respond when participants omit response?
                                // Wait for key press and trigger partner finger return animation
                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                partnerAnimator.SetBool("return", true);

                                // Reset animation transitions
                                yield return null;
                                partnerAnimator.SetBool("return", false);
                                */
                            }
                        }
                        // catch trial 4
                        else if (trialListGroupInd[i] == 4)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_2;
                            //ColorCue.SetActive(true);
                            //SquareStim.SetActive(true);
                            MiddleShape.SetActive(true);
                            Cue.text = MiddleCue;
                            thisTrial.trialCondition = 4;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger participant finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift

                                    // Trigger partner finger lift after variable delay
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Trigger THEY finger lift
                                    if (imitatingAvatarListGroupInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar1Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        partnerAnimator.SetInteger("liftFinger", 0);
                                        Avatar1Animator.SetInteger("catch", 0);
                                    }
                                    else if (imitatingAvatarListGroupInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar2Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay);

                                        // Reset animation transitions
                                        participantAnimator.SetInteger("liftFinger", 0);
                                        partnerAnimator.SetInteger("liftFinger", 0);
                                        Avatar2Animator.SetInteger("catch", 0);
                                    }


                                    // Wait for catch trial response
                                    referenceTime = Time.time;
                                    while ((Time.time - referenceTime) <= ResponseWindowCatch)
                                    {
                                        yield return null;
                                        if (!Input.GetKey(IndexKey))
                                        {
                                            thisTrial.catchTrialResponse = true;
                                            yield return new WaitForSeconds(ResponseWindowCatch - (Time.time - referenceTime));
                                            break;
                                        }
                                        else if (Input.GetKey(IndexKey))
                                        {
                                            thisTrial.catchTrialResponse = false;
                                        }
                                    }

                                    if (thisTrial.catchTrialResponse == false)
                                    {
                                        Cue.fontSize = CatchPromptFontSize;
                                        Cue.text = CatchPrompt;
                                        Cue.enabled = true;
                                        yield return new WaitForSeconds(CatchFeedbackDisplay);
                                    }

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (imitatingAvatarListGroupInd[i] == 1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar1Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        partnerAnimator.SetBool("return", false);
                                        Avatar1Animator.SetBool("return", false);
                                    }
                                    else if (imitatingAvatarListGroupInd[i] == 2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar2Animator.SetBool("return", true);

                                        // Reset animation transitions
                                        yield return null;
                                        participantAnimator.SetBool("return", false);
                                        partnerAnimator.SetBool("return", false);
                                        Avatar2Animator.SetBool("return", false);
                                    }

                                    break;
                                }
                                else if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // wrong response
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    // record/update response data
                                    i--; // repeat trial --> double check if this works as intended!
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger partner finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger partner finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            // if no response was recorded during response window, display error message
                            if (thisTrial.response == "none")
                            {
                                i--; // repeat trial --> double check if this works as intended!

                                /* TBD --> should partner respond when participants omit response?
                                // Trigger partner finger lift
                                partnerAnimator.SetInteger("liftFinger", 1);
                                yield return new WaitForSeconds(ErrorFeedbackDelay); // delay slow feedback
                                partnerAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                */

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //SquareStim.SetActive(false);
                                MiddleShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                /* TBD --> should partner respond when participants omit response?
                                // Wait for key press and trigger partner finger return animation
                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                partnerAnimator.SetBool("return", true);

                                // Reset animation transitions
                                yield return null;
                                partnerAnimator.SetBool("return", false);
                                */
                            }
                        }
                    }
                    Debug.Log(thisTrial.ToString());
                    experimentData.Add(thisTrial);
                    yield return new WaitForSeconds(ITI); // ITI
                    if (i == FamTrialN - 1 && (thisTrial.trialCondition == 1 || thisTrial.trialCondition == 2))
                    {
                        // Show instructions again after training
                        Cue.enabled = false;
                        //ColorCue.SetActive(false);
                        //CircleStim.SetActive(false);
                        //SquareStim.SetActive(false);
                        IndexShape.SetActive(false);
                        MiddleShape.SetActive(false);
                        Instructions.text = InstructionTextTest;
                        Instructions.enabled = true;
                        yield return new WaitUntil(() => Input.GetKey("space"));
                        Instructions.enabled = false;
                        yield return new WaitForSeconds(0.5f);
                    }
                    if (thisTrial.trialCounter == trialListGroupInd.Length / 2 && (thisTrial.trialCondition == 1 || thisTrial.trialCondition == 2))
                    {
                        // Show instructions about imitating avatar change after first half of block
                        Cue.enabled = false;
                        IndexShape.SetActive(false);
                        MiddleShape.SetActive(false);
                        Instructions.text = ImitatingAvatarInstructionsTextSecondHalf;
                        Instructions.enabled = true;
                        yield return new WaitUntil(() => Input.GetKey("space"));
                        Instructions.enabled = false;
                        yield return new WaitForSeconds(0.5f);
                    }
                    if (i == (trialListGroupInd.Length / 2 + FamTrialN) - 1 && (thisTrial.trialCondition == 1 || thisTrial.trialCondition == 2))
                    {
                        // Show instructions again after training
                        Cue.enabled = false;
                        //ColorCue.SetActive(false);
                        //CircleStim.SetActive(false);
                        //SquareStim.SetActive(false);
                        IndexShape.SetActive(false);
                        MiddleShape.SetActive(false);
                        Instructions.text = InstructionTextTest;
                        Instructions.enabled = true;
                        yield return new WaitUntil(() => Input.GetKey("space"));
                        Instructions.enabled = false;
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                break; 

            case "GroupGroup":

                // Show instructions
                Cue.enabled = false;
                //ColorCue.SetActive(false);
                //CircleStim.SetActive(false);
                //SquareStim.SetActive(false);
                IndexShape.SetActive(false);
                MiddleShape.SetActive(false);
                Instructions.text = "\r\n\r\n<u><b>" + partText + InstructionTextGroupGroup;
                Instructions.enabled = true;
                yield return new WaitUntil(() => Input.GetKey("space"));
                Instructions.enabled = false;
                yield return new WaitForSeconds(0.5f);

                // Init block
                int[] trialListGroupGroupPractice = trialRandomizer(FamTrialN, 0);
                int[] trialListGroupGroupTest = testTrialRandomizer(TrialN, CatchTrialN); // randomize trial conditions

                int halfTrialsGroupGroup = trialListGroupGroupTest.Length / 2;
                int[] trialListGroupGroupTestFirstHalf = trialListGroupGroupTest.Take(halfTrialsGroupGroup).ToArray();
                int[] trialListGroupGroupTestSecondHalf = trialListGroupGroupTest.Skip(halfTrialsGroupGroup).ToArray();

                int[] trialListGroupGroupFirstHalf = ConcatenateTrials(trialListGroupGroupPractice, trialListGroupGroupTestFirstHalf);
                int[] trialListGroupGroupSecondHalf = ConcatenateTrials(trialListGroupGroupPractice, trialListGroupGroupTestSecondHalf);

                int[] trialListGroupGroup = ConcatenateTrials(trialListGroupGroupFirstHalf, trialListGroupGroupSecondHalf);

                arrayAsString = string.Join(", ", trialListGroupGroup);
                Debug.Log(arrayAsString);

                for (int i = 0; i < (trialListGroupGroup.Length); i++) // loop over # of trials
                {
                    // init TrialData class to save trial data
                    TrialData thisTrial = new TrialData();
                    thisTrial.ParticipantID = ParticipantID;
                    thisTrial.HandPosition = HandPosition;
                    thisTrial.blockCounter = blockCounter;
                    thisTrial.blockCondition = blockCondition;
                    thisTrial.blockOrder = blockOrder;
                    thisTrial.SRMapping = SRMapping;
                    thisTrial.imitationDelayAvatar1 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.imitationDelayAvatar2 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.returnDelayAvatar1 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.returnDelayAvatar2 = UnityEngine.Random.Range(ImitationDelayMin, ImitationDelayMax);
                    thisTrial.responseDelayPartner = UnityEngine.Random.Range(PartnerResponseDelayMin, PartnerResponseDelayMax);
                    thisTrial.returnDelayPartner = UnityEngine.Random.Range(PartnerResponseDelayMin, PartnerResponseDelayMax);
                    thisTrial.imitatingAvatar = "both";
                    //thisTrial.responseTimePartner = UnityEngine.Random.Range(PartnerRTMin, PartnerRTMax);

                    // Init trial variables
                    thisTrial.trialCounter = i + 1;
                    Debug.Log("Trial #: " + thisTrial.trialCounter);
                    thisTrial.response = null;
                    thisTrial.responseTime = 0;
                    thisTrial.responseCorrect = false;

                    // Display hold prompt and wait until both keys are pressed
                    Cue.text = HoldPrompt;
                    Cue.fontSize = PromptFontSize;
                    Cue.enabled = true;
                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                    // Display fixation cross after delay and listen for key release before cue onset
                    referenceTime = Time.time;
                    while ((Time.time - referenceTime) < (FixationCrossDelay + FixationCrossDisplay)) // Input.GetKey(IndexKey) && Input.GetKey(MiddleKey) && (
                    {
                        yield return null;
                        if (!Input.GetKey(IndexKey) || !Input.GetKey(MiddleKey))
                        {
                            thisTrial.response = "early";
                            break;
                        }
                        if (Time.time - referenceTime >= FixationCrossDelay)
                        {
                            Cue.fontSize = CueFontSize;
                            Cue.text = "+";
                        }
                    }
                    // If response was made before cue presentation --> skip/repeat trial
                    if (thisTrial.response == "early")
                    {
                        i--; // repeat trial --> double check if this works as intended!
                        Cue.fontSize = PromptFontSize;
                        Cue.text = EarlyMessage;
                        Cue.enabled = true;
                        yield return new WaitForSeconds(ErrorFeedbackDisplay);

                        // Display hold prompt
                        Cue.text = HoldPrompt;
                        yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                    }

                    else if (thisTrial.response == null)
                    {
                        if (trialListGroupGroup[i] == 1)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_1;
                            //ColorCue.SetActive(true);
                            //CircleStim.SetActive(true);
                            IndexShape.SetActive(true);
                            Cue.text = IndexCue;
                            thisTrial.trialCondition = 1;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger participant finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift

                                    // Trigger partner finger lift after variable delay
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Trigger THEY finger lift
                                    if (thisTrial.imitationDelayAvatar1 <= thisTrial.imitationDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar1Animator.SetInteger("liftFinger", 1);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.imitationDelayAvatar1);
                                        Avatar2Animator.SetInteger("liftFinger", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }
                                    else if (thisTrial.imitationDelayAvatar2 <= thisTrial.imitationDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar2Animator.SetInteger("liftFinger", 1);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.imitationDelayAvatar2);
                                        Avatar1Animator.SetInteger("liftFinger", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0);
                                    partnerAnimator.SetInteger("liftFinger", 0);
                                    Avatar1Animator.SetInteger("liftFinger", 0);
                                    Avatar2Animator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (thisTrial.returnDelayAvatar1 <= thisTrial.returnDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar1Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);
                                    }
                                    else if (thisTrial.returnDelayAvatar2 <= thisTrial.returnDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayPartner);
                                        Avatar2Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayAvatar2);
                                        Avatar1Animator.SetBool("return", true);
                                    }

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);
                                    Avatar1Animator.SetBool("return", false);
                                    Avatar2Animator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // wrong response
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger partner finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger partner finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            // if no response was recorded during response window, display error message
                            if (thisTrial.response == "none")
                            {
                                /* TBD --> should partner respond when participants omit response?
                                // Trigger partner finger lift
                                partnerAnimator.SetInteger("liftFinger", 1);
                                yield return new WaitForSeconds(ErrorFeedbackDelay); // delay slow feedback
                                partnerAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                */
                                if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                {
                                    i--; // repeat training trial if error was made
                                }

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //CircleStim.SetActive(false);
                                IndexShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                /* TBD --> should partner respond when participants omit response?
                                // Wait for key press and trigger partner finger return animation
                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                partnerAnimator.SetBool("return", true);

                                // Reset animation transitions
                                yield return null;
                                partnerAnimator.SetBool("return", false);
                                */
                            }
                        }
                        else if (trialListGroupGroup[i] == 2)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_2;
                            //ColorCue.SetActive(true);
                            //SquareStim.SetActive(true);
                            MiddleShape.SetActive(true);
                            Cue.text = MiddleCue;
                            thisTrial.trialCondition = 2;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger participant finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 1 --> index finger lift

                                    // Trigger partner finger lift after variable delay
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Trigger THEY finger lift
                                    if (thisTrial.imitationDelayAvatar1 <= thisTrial.imitationDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar1Animator.SetInteger("liftFinger", 2);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.imitationDelayAvatar1);
                                        Avatar2Animator.SetInteger("liftFinger", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }
                                    else if (thisTrial.imitationDelayAvatar2 <= thisTrial.imitationDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar2Animator.SetInteger("liftFinger", 2);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.imitationDelayAvatar2);
                                        Avatar1Animator.SetInteger("liftFinger", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0);
                                    partnerAnimator.SetInteger("liftFinger", 0);
                                    Avatar1Animator.SetInteger("liftFinger", 0);
                                    Avatar2Animator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (thisTrial.returnDelayAvatar1 <= thisTrial.returnDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar1Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);
                                    }
                                    else if (thisTrial.returnDelayAvatar2 <= thisTrial.returnDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayPartner);
                                        Avatar2Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayAvatar2);
                                        Avatar1Animator.SetBool("return", true);
                                    }

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);
                                    Avatar1Animator.SetBool("return", false);
                                    Avatar2Animator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // wrong response
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 2 --> middle finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                    {
                                        i--; // repeat training trial if error was made
                                    }

                                    // record/update response data
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger partner finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger partner finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            // if no response was recorded during response window, display error message
                            if (thisTrial.response == "none")
                            {
                                /* TBD --> should partner respond when participants omit response?
                                // Trigger partner finger lift
                                partnerAnimator.SetInteger("liftFinger", 1);
                                yield return new WaitForSeconds(ErrorFeedbackDelay); // delay slow feedback
                                partnerAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                */
                                if (thisTrial.trialCounter == 1 || thisTrial.trialCounter == 2)
                                {
                                    i--; // repeat training trial if error was made
                                }

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //SquareStim.SetActive(false);
                                MiddleShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                /* TBD --> should partner respond when participants omit response?
                                // Wait for key press and trigger partner finger return animation
                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                partnerAnimator.SetBool("return", true);

                                // Reset animation transitions
                                yield return null;
                                partnerAnimator.SetBool("return", false);
                                */
                            }
                        }
                        // catch trial 3
                        else if (trialListGroupGroup[i] == 3)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_1;
                            //ColorCue.SetActive(true);
                            //CircleStim.SetActive(true);
                            IndexShape.SetActive(true);
                            Cue.text = IndexCue;
                            thisTrial.trialCondition = 3;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger participant finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift

                                    // Trigger partner finger lift after variable delay
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Trigger THEY finger lift
                                    if (thisTrial.imitationDelayAvatar1 <= thisTrial.imitationDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar1Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.imitationDelayAvatar1);
                                        Avatar2Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }
                                    else if (thisTrial.imitationDelayAvatar2 <= thisTrial.imitationDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar2Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.imitationDelayAvatar2);
                                        Avatar1Animator.SetInteger("catch", 1);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }

                                    // Wait for catch trial response
                                    referenceTime = Time.time;
                                    while ((Time.time - referenceTime) <= ResponseWindowCatch)
                                    {
                                        yield return null;
                                        if (!Input.GetKey(MiddleKey))
                                        {
                                            thisTrial.catchTrialResponse = true;
                                            yield return new WaitForSeconds(ResponseWindowCatch - (Time.time - referenceTime));
                                            break;
                                        }
                                        else if (Input.GetKey(MiddleKey))
                                        {
                                            thisTrial.catchTrialResponse = false;
                                        }
                                    }

                                    if (thisTrial.catchTrialResponse == false)
                                    {
                                        Cue.fontSize = CatchPromptFontSize;
                                        Cue.text = CatchPrompt;
                                        Cue.enabled = true;
                                        yield return new WaitForSeconds(CatchFeedbackDisplay);
                                    }

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0);
                                    partnerAnimator.SetInteger("liftFinger", 0);
                                    Avatar1Animator.SetInteger("catch", 0);
                                    Avatar2Animator.SetInteger("catch", 0);

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (thisTrial.returnDelayAvatar1 <= thisTrial.returnDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar1Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);
                                    }
                                    else if (thisTrial.returnDelayAvatar2 <= thisTrial.returnDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayPartner);
                                        Avatar2Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayAvatar2);
                                        Avatar1Animator.SetBool("return", true);
                                    }

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);
                                    Avatar1Animator.SetBool("return", false);
                                    Avatar2Animator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // wrong response
                                {
                                    // record/update response data
                                    i--; // repeat catch trial
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 2 --> middle finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    // record/update response data
                                    i--; // repeat catch trial
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //CircleStim.SetActive(false);
                                    IndexShape.SetActive(false);

                                    // Trigger partner finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 1);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger partner finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            // if no response was recorded during response window, display error message
                            if (thisTrial.response == "none")
                            {
                                i--; // repeat catch trial

                                /* TBD --> should partner respond when participants omit response?
                                // Trigger partner finger lift
                                partnerAnimator.SetInteger("liftFinger", 1);
                                yield return new WaitForSeconds(ErrorFeedbackDelay); // delay slow feedback
                                partnerAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                */

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //CircleStim.SetActive(false);
                                IndexShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                /* TBD --> should partner respond when participants omit response?
                                // Wait for key press and trigger partner finger return animation
                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                partnerAnimator.SetBool("return", true);

                                // Reset animation transitions
                                yield return null;
                                partnerAnimator.SetBool("return", false);
                                */
                            }
                        }
                        // catch trial 4
                        else if (trialListGroupGroup[i] == 4)
                        {
                            // Display cue and wait for key release
                            //ColorCueRenderer.material.color = ColorCue_2;
                            //ColorCue.SetActive(true);
                            //SquareStim.SetActive(true);
                            MiddleShape.SetActive(true);
                            Cue.text = MiddleCue;
                            thisTrial.trialCondition = 4;

                            referenceTime = Time.time;
                            while ((Time.time - referenceTime) <= ResponseWindow) // listen for keyboard input
                            {
                                yield return null;
                                if (!Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // correct response
                                {
                                    // record/update response data
                                    thisTrial.response = MiddleKey;
                                    thisTrial.responseCorrect = true;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger participant finger lift
                                    participantAnimator.SetInteger("liftFinger", 2); // 1 --> index finger lift

                                    // Trigger partner finger lift after variable delay
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Trigger THEY finger lift
                                    if (thisTrial.imitationDelayAvatar1 <= thisTrial.imitationDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar1Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.imitationDelayAvatar1);
                                        Avatar2Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }
                                    else if (thisTrial.imitationDelayAvatar2 <= thisTrial.imitationDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar2 - thisTrial.responseDelayPartner); // Imitation delay
                                        Avatar2Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(thisTrial.imitationDelayAvatar1 - thisTrial.imitationDelayAvatar2);
                                        Avatar1Animator.SetInteger("catch", 2);
                                        yield return new WaitForSeconds(OutcomeDisplay); // Duration of outcome display
                                    }

                                    // Wait for catch trial response
                                    referenceTime = Time.time;
                                    while ((Time.time - referenceTime) <= ResponseWindowCatch)
                                    {
                                        yield return null;
                                        if (!Input.GetKey(IndexKey))
                                        {
                                            thisTrial.catchTrialResponse = true;
                                            yield return new WaitForSeconds(ResponseWindowCatch - (Time.time - referenceTime));
                                            break;
                                        }
                                        else if (Input.GetKey(IndexKey))
                                        {
                                            thisTrial.catchTrialResponse = false;
                                        }
                                    }

                                    if (thisTrial.catchTrialResponse == false)
                                    {
                                        Cue.fontSize = CatchPromptFontSize;
                                        Cue.text = CatchPrompt;
                                        Cue.enabled = true;
                                        yield return new WaitForSeconds(CatchFeedbackDisplay);
                                    }

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0);
                                    partnerAnimator.SetInteger("liftFinger", 0);
                                    Avatar1Animator.SetInteger("catch", 0);
                                    Avatar2Animator.SetInteger("catch", 0);

                                    // Display hold prompt
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = HoldPrompt;
                                    Cue.enabled = true;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(MiddleKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Trigger THEY finger return animation
                                    if (thisTrial.returnDelayAvatar1 <= thisTrial.returnDelayAvatar2)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayPartner);
                                        Avatar1Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayAvatar1);
                                        Avatar2Animator.SetBool("return", true);
                                    }
                                    else if (thisTrial.returnDelayAvatar2 <= thisTrial.returnDelayAvatar1)
                                    {
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar2 - thisTrial.returnDelayPartner);
                                        Avatar2Animator.SetBool("return", true);
                                        yield return new WaitForSeconds(thisTrial.returnDelayAvatar1 - thisTrial.returnDelayAvatar2);
                                        Avatar1Animator.SetBool("return", true);
                                    }

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);
                                    Avatar1Animator.SetBool("return", false);
                                    Avatar2Animator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(IndexKey) && Input.GetKey(MiddleKey)) // wrong response
                                {
                                    // record/update response data
                                    i--; // repeat catch trial
                                    thisTrial.response = IndexKey;
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger WE finger lift
                                    participantAnimator.SetInteger("liftFinger", 1); // 1 --> index finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger WE finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey));
                                    participantAnimator.SetBool("return", true);
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    participantAnimator.SetBool("return", false);
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (!Input.GetKey(MiddleKey) && !Input.GetKey(IndexKey)) // double response
                                {
                                    // record/update response data
                                    i--; // repeat catch trial
                                    thisTrial.response = "double";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = Time.time - referenceTime;
                                    Cue.enabled = false;
                                    //ColorCue.SetActive(false);
                                    //SquareStim.SetActive(false);
                                    MiddleShape.SetActive(false);

                                    // Trigger partner finger lift
                                    yield return new WaitForSeconds(thisTrial.responseDelayPartner);
                                    partnerAnimator.SetInteger("liftFinger", 2);

                                    // Display error feedback
                                    AlarmRenderer.material.color = AlarmColorOn; // light turns red
                                    AlarmLight.enabled = true;
                                    Cue.fontSize = PromptFontSize;
                                    Cue.text = ErrorMessage;
                                    Cue.enabled = true;
                                    yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                    // Reset animation transitions
                                    participantAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                    partnerAnimator.SetInteger("liftFinger", 0);

                                    // Display hold prompt
                                    AlarmRenderer.material.color = AlarmColorOff;
                                    AlarmLight.enabled = false;
                                    Cue.text = HoldPrompt;

                                    // Wait for key press and trigger partner finger return animation
                                    yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                    yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                    partnerAnimator.SetBool("return", true);

                                    // Reset animation transitions
                                    yield return null;
                                    partnerAnimator.SetBool("return", false);

                                    break;
                                }
                                else if (Input.GetKey(MiddleKey) && Input.GetKey(IndexKey)) // no response
                                {
                                    thisTrial.response = "none";
                                    thisTrial.responseCorrect = false;
                                    thisTrial.responseTime = 0;
                                }
                            }
                            // if no response was recorded during response window, display error message
                            if (thisTrial.response == "none")
                            {
                                i--; // repeat catch trial

                                /* TBD --> should partner respond when participants omit response?
                                // Trigger partner finger lift
                                partnerAnimator.SetInteger("liftFinger", 1);
                                yield return new WaitForSeconds(ErrorFeedbackDelay); // delay slow feedback
                                partnerAnimator.SetInteger("liftFinger", 0); // reset animation transition
                                */

                                Cue.fontSize = PromptFontSize;
                                Cue.text = SlowMessage;
                                Cue.enabled = true;
                                //ColorCue.SetActive(false);
                                //SquareStim.SetActive(false);
                                MiddleShape.SetActive(false);
                                yield return new WaitForSeconds(ErrorFeedbackDisplay);

                                // Display hold prompt
                                AlarmRenderer.material.color = AlarmColorOff;
                                AlarmLight.enabled = false;
                                Cue.text = HoldPrompt;

                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));

                                /* TBD --> should partner respond when participants omit response?
                                // Wait for key press and trigger partner finger return animation
                                yield return new WaitUntil(() => Input.GetKey(IndexKey) && Input.GetKey(MiddleKey));
                                yield return new WaitForSeconds(thisTrial.returnDelayPartner); // delay partner return
                                partnerAnimator.SetBool("return", true);

                                // Reset animation transitions
                                yield return null;
                                partnerAnimator.SetBool("return", false);
                                */
                            }
                        }
                    }
                    Debug.Log(thisTrial.ToString());
                    experimentData.Add(thisTrial);
                    yield return new WaitForSeconds(ITI); // ITI
                    if (i == FamTrialN-1 && (thisTrial.trialCondition == 1 || thisTrial.trialCondition == 2))
                    {
                        // Show instructions again after training
                        Cue.enabled = false;
                        //ColorCue.SetActive(false);
                        //CircleStim.SetActive(false);
                        //SquareStim.SetActive(false);
                        IndexShape.SetActive(false);
                        MiddleShape.SetActive(false);
                        Instructions.text = InstructionTextTest;
                        Instructions.enabled = true;
                        yield return new WaitUntil(() => Input.GetKey("space"));
                        Instructions.enabled = false;
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                break;
        }
    }

    IEnumerator ExpProcedure()
    {
        // Pick random S-R mapping
        //int index = UnityEngine.Random.Range(0, 2); // random draw
        int index = 0; // for specific SRMapping
        switch (index)
        {
            case 0:
                SRMapping = "A";
                IndexShape = CircleStim;
                MiddleShape = SquareStim;
                break;
            case 1:
                SRMapping = "B";
                IndexShape = SquareStim;
                MiddleShape = CircleStim;
                break;
        }

        // Pick random order of first imitator
        int index2 = UnityEngine.Random.Range(0, 2);
        switch (index2)
        {
            case 0:
                firstImitator = "right";
                ImitatingAvatarInstructionsTextFirstHalf = "\r\n\r\nIn the first half of this part, the <b>upper right person</b> will imitate your finger lift responses.\r\n\r\nYou will start with two training trials to get familiar with the procedure.\r\n\r\nPress[space] to start.";
                ImitatingAvatarInstructionsTextSecondHalf = "\r\n\r\n\r\n\r\nYou have completed the first\r\nhalf of this part.\r\n\r\nIn the second half of this part, now the <b>upper left person</b> will imitate your finger lift responses.\r\n\r\nLet's start with two training trials to get familiar with the procedure.\r\n\r\nPress[space] to start.";
                break;
            case 1:
                firstImitator = "left";
                ImitatingAvatarInstructionsTextFirstHalf = "\r\n\r\nIn the first half of this part, the <b>upper left person</b> will imitate your finger lift responses.\r\n\r\nYou will start with two training trials to get familiar with the procedure.\r\n\r\nPress[space] to start.";
                ImitatingAvatarInstructionsTextSecondHalf = "\r\n\r\n\r\n\r\nYou have completed the first\r\nhalf of this part.\r\n\r\nIn the second half of this part, now the <b>upper right person</b> will imitate your finger lift responses.\r\n\r\nLet's start with two training trials to get familiar with the procedure.\r\n\r\nPress[space] to start.";
                break;
        }

        // Start Task Instructions Procedure
        yield return StartCoroutine(TaskInstructions());

        // Start Ind Familiarization Procedure
        yield return StartCoroutine(PracticeIndTrials(PracticeTrialN));

        // Start Joint Familiarization Procedure
        yield return StartCoroutine(PracticeGroupTrials(PracticeTrialN));

        // Start Imitation Instructions Procedure
        yield return StartCoroutine(ImitationInstructions());

        // Start Main Experimental Procedure

        // Activate avatars
        Agents[0].SetActive(true); // Participant Hand
        Agents[1].SetActive(true); // Partner Hand
        Agents[2].SetActive(true); // Avatar1
        Agents[3].SetActive(true); // Avatar2

        // Randomly pick condition order from the balanced latin square 
        List<string> conditionOrder = GetRandomConditionOrder();

        // Init. block counter
        blockCounter = 0;
        
        // Loop through conditions
        foreach (string condition in conditionOrder)
        {
            blockCounter = blockCounter + 1; // count block
            blockCondition = condition; // save block condition
            
            switch (blockCounter) // For instructions
            {
                case 1:
                    partText = "First part";
                    break;
                case 2:
                    partText = "Second part";
                    break;
                case 3:
                    partText = "Third part";
                    break;
                case 4:
                    partText = "Fourth part";
                    break;
            }
            Debug.Log("Starting animation with parameter: " + condition);
            Debug.Log("Block Order: " + blockOrder);
            yield return StartCoroutine(TrialProcedure(condition, FamTrialN, TrialN, CatchTrialN));
        }

        // Send data to server
        string jsonExperimentData = SerializeTrialDataListToJson(experimentData);
        JatosInterface.SendResultDataToJatos(jsonExperimentData);

        // Display end message
        Cue.enabled = false;
        //ColorCue.SetActive(false);
        //CircleStim.SetActive(false);
        //SquareStim.SetActive(false);
        IndexShape.SetActive(false);
        MiddleShape.SetActive(false);
        Agents[0].SetActive(false); // Participant Hand
        Agents[1].SetActive(false); // Partner Hand
        Agents[2].SetActive(false); // Avatar1
        Agents[3].SetActive(false); // Avatar2
        Instructions.text = EndText;
        Instructions.enabled = true;
        yield return new WaitUntil(() => Input.GetKey("space"));
        Instructions.enabled = false;
        yield return new WaitForSeconds(0.5f);

        // End experiment / start next jatos component
        JatosInterface.StartNextJatosEvent();

    }
    #endregion

    #region Define helper functions

    // Function to generate random participant ID
    public static int GenerateParticipantID(int minLength)
    {
        int start = (int)Mathf.Pow(10, minLength - 1); // Determine the start of the range.
        int end = (int)Mathf.Pow(10, minLength); // Determine the end of the range, exclusive.
        return Random.Range(start, end); // Generate and return a random number within the range.
    }

    // Function for randomizing trial conditions
    static int[] trialRandomizer(int TrialN, int CatchTrialN)
    {
        int[] trialList = new int[TrialN + CatchTrialN];

        for (int i = 0; i < TrialN / 2; i++)
        {
            trialList[i] = 1;
            trialList[i + TrialN / 2] = 2;
        }

        // Add catch trials (3s and 4s) to the end of the array
        for (int i = 0; i < CatchTrialN / 2; i++)
        {
            trialList[TrialN + i] = 3; // First half of CatchTrialN as 3s
        }
        for (int i = 0; i < CatchTrialN / 2; i++)
        {
            trialList[TrialN + CatchTrialN / 2 + i] = 4; // Second half of CatchTrialN as 4s
        }

        for (int i = (TrialN+CatchTrialN) - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = trialList[i];
            trialList[i] = trialList[j];
            trialList[j] = temp;
        }

        return trialList;
    }

    static int[] testTrialRandomizer(int TrialN, int CatchTrialN)
    {
        int[] trialList = new int[TrialN + CatchTrialN]; // array for full trial sequence

        int totalTrials = TrialN + CatchTrialN;
        int halfTrials = (TrialN + CatchTrialN) / 2;
        int halfMainTrials = TrialN / 2;
        int halfCatchTrials = CatchTrialN / 2;

        // Populate the first half with main trials
        for (int i = 0; i < halfMainTrials / 2; i++)
        {
            trialList[i] = 1; // First half of 1s
            trialList[i + halfMainTrials / 2] = 2; // First half of 2s
        }

        // Populate the first half with catch trials
        for (int i = 0; i < halfCatchTrials / 2; i++)
        {
            trialList[halfMainTrials + i] = 3; // First half of 3s
            trialList[halfMainTrials + halfCatchTrials / 2 + i] = 4; // First half of 4s
        }

        // Populate the second half with main trials 
        for (int i = 0; i < halfMainTrials / 2; i++)
        {
            trialList[halfTrials + i] = 1; // Second half of 1s
            trialList[halfTrials + halfMainTrials / 2 + i] = 2; // Second half of 2s
        }

        // Populate the second half with catch trials 
        for (int i = 0; i < halfCatchTrials / 2; i++)
        {
            trialList[halfTrials + halfMainTrials + i] = 3; // Second half of 3s
            trialList[halfTrials + halfMainTrials + halfCatchTrials / 2 + i] = 4; // Second half of 4s
        }

        // Shuffle the first half
        for (int i = halfTrials - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = trialList[i];
            trialList[i] = trialList[j];
            trialList[j] = temp;
        }

        // Shuffle the second half
        for (int i = totalTrials - 1; i >= halfTrials; i--)
        {
            int j = UnityEngine.Random.Range(halfTrials, i + 1);
            int temp = trialList[i];
            trialList[i] = trialList[j];
            trialList[j] = temp;
        }

        return trialList;

    }

    // Function for generating list of imitating avatar in ONE blocks
    static int[] GenerateAvatarList(int[] trialList, string firstImitator)
    {
        int totalTrials = trialList.Length;
        int halfTrials = totalTrials / 2;

        int[] avatarList = new int[totalTrials];

        // Populate array
        if (firstImitator == "right")
        {
            for (int i = 0; i < halfTrials; i++)
            {
                avatarList[i] = 1; // First half of 1s
                avatarList[i + halfTrials] = 2; // First half of 2s
            }
        }
        else if (firstImitator == "left")
        {
            for (int i = 0; i < halfTrials; i++)
            {
                avatarList[i] = 2; // First half of 1s
                avatarList[i + halfTrials] = 1; // First half of 2s
            }
        }

        return avatarList;
    }


    /*
    // Function for randomizing imitating avatar in ONE blocks
    static int[] GenerateAvatarList(int[] trialList, int PracticeTrialN)
    {
        int totalTrials = trialList.Length;
        int halfTrials = totalTrials / 2;
        int[] avatarList = new int[totalTrials + PracticeTrialN]; // Final list including practice trials

        // Step 1: Create the practiceList with equal numbers of 1s and 2s
        int[] practiceList = new int[PracticeTrialN];
        for (int i = 0; i < PracticeTrialN / 2; i++)
        {
            practiceList[i] = 1;
            practiceList[i + PracticeTrialN / 2] = 2;
        }

        // Step 2: Shuffle practiceList
        for (int i = PracticeTrialN - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = practiceList[i];
            practiceList[i] = practiceList[j];
            practiceList[j] = temp;
        }

        // Step 3: Create dictionaries for storing indices of each trial condition separately for each half
        Dictionary<int, List<int>> firstHalfIndices = new Dictionary<int, List<int>>()
        {
            {1, new List<int>()},
            {2, new List<int>()}
        };

        Dictionary<int, List<int>> secondHalfIndices = new Dictionary<int, List<int>>()
        {
            {1, new List<int>()},
            {2, new List<int>()}
        };

        // Populate dictionaries with the respective indices
        for (int i = 0; i < totalTrials; i++)
        {
            if (i < halfTrials) // First half
            {
                if (trialList[i] == 1 || trialList[i] == 3)
                {
                    firstHalfIndices[1].Add(i + PracticeTrialN);
                }
                else if (trialList[i] == 2 || trialList[i] == 4)
                {
                    firstHalfIndices[2].Add(i + PracticeTrialN);
                }
            }
            else // Second half
            {
                if (trialList[i] == 1 || trialList[i] == 3)
                {
                    secondHalfIndices[1].Add(i + PracticeTrialN);
                }
                else if (trialList[i] == 2 || trialList[i] == 4)
                {
                    secondHalfIndices[2].Add(i + PracticeTrialN);
                }
            }
        }

        // Step 4: Assign balanced 1s and 2s within each half
        foreach (var condition in firstHalfIndices.Keys)
        {
            // Process first half
            List<int> firstIndices = firstHalfIndices[condition];
            int firstHalfSize = firstIndices.Count / 2;

            for (int i = 0; i < firstHalfSize; i++)
            {
                avatarList[firstIndices[i]] = 1;
            }
            for (int i = firstHalfSize; i < firstIndices.Count; i++)
            {
                avatarList[firstIndices[i]] = 2;
            }

            // Shuffle 1s and 2s within the first half
            for (int i = firstIndices.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = avatarList[firstIndices[i]];
                avatarList[firstIndices[i]] = avatarList[firstIndices[j]];
                avatarList[firstIndices[j]] = temp;
            }

            // Process second half
            List<int> secondIndices = secondHalfIndices[condition];
            int secondHalfSize = secondIndices.Count / 2;

            for (int i = 0; i < secondHalfSize; i++)
            {
                avatarList[secondIndices[i]] = 1;
            }
            for (int i = secondHalfSize; i < secondIndices.Count; i++)
            {
                avatarList[secondIndices[i]] = 2;
            }

            // Shuffle 1s and 2s within the second half
            for (int i = secondIndices.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = avatarList[secondIndices[i]];
                avatarList[secondIndices[i]] = avatarList[secondIndices[j]];
                avatarList[secondIndices[j]] = temp;
            }
        }

        // Step 5: Add practice trials to the start of avatarList
        for (int i = 0; i < PracticeTrialN; i++)
        {
            avatarList[i] = practiceList[i];
        }

        return avatarList;
    } */

    // Function for concatenating practice a dn test trials
    static int[] ConcatenateTrials(int[] practiceTrials, int[] testTrials)
    {
        int[] concatenatedTrials = new int[practiceTrials.Length + testTrials.Length];

        // Copy the first array elements to the new array
        for (int i = 0; i < practiceTrials.Length; i++)
        {
            concatenatedTrials[i] = practiceTrials[i];
        }

        // Copy the second array elements to the new array, starting at the end of the first
        for (int i = 0; i < testTrials.Length; i++)
        {
            concatenatedTrials[practiceTrials.Length + i] = testTrials[i];
        }

        return concatenatedTrials;
    }

    // Function to select a random condition order
    public List<string> GetRandomConditionOrder()
    {
        //int index = UnityEngine.Random.Range(1, 4); // random draw
        int index = 3; // for collecting participants in specific block order condition
        switch (index)
        {
            case 0:
                blockOrder = "A";
                break;
            case 1:
                blockOrder = "B";
                break;
            case 2:
                blockOrder = "C";
                break;
            case 3:
                blockOrder = "D";
                break;
        }
        return conditionOrders[index];
    }

    // Funtion for shuffling block conditions
    static void ShuffleBlockConditions<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // Function to serialize experiment data structure to json string for server export
    public static string SerializeTrialDataListToJson(List<TrialData> trialDataList)
    {
        TrialDataList wrapper = new TrialDataList
        {
            trialDataList = trialDataList
        };

        return JsonUtility.ToJson(wrapper, true);
    }

    #endregion


}
