mergeInto(LibraryManager.library, {
    
    // Save results to JATOS
    sendResultDataToJatos: function(utf8String) 
    {
        var jsString = UTF8ToString(utf8String);
        sendResultData(jsString);
    },
    
    // Move to the next JATOS block
    startNextJatosEvent: function() 
    {
        startNextJatosComponent();
    },
});