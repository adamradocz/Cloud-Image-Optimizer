jQuery(document).ready(function ($)
{
    "use strict";

    $('#iio-validate-api-key').click(function ()
    {
        var data =
        {
            'action' : 'ValidateApiKey',
            'nonce' : ajax_object.nonce
        };

        $(this).hide(); // Hide the "Validate" button
        $(this).after(ajax_object.loaderImageHtml); // Show the loader image

        $.post(ajax_object.ajax_url, data, function (responseData, status)
        {
            $('#iio-validate-api-key').show();  // Show "Validate" button
            $('#iio-loading-gif').hide();       // Hide loading image
            $('#iio-api-key-status').after(responseData.html).remove(); // Change the information text of the 'API Key Status'
        });
    });
});