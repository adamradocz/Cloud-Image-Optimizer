jQuery(document).ready(function ($)
{
    "use strict";

    $('.iio-optimize-now').click(function ()
    {
        var data =
        {
            'action' : 'OptimizeMediaLibraryImage',
            'nonce' : ajax_object.nonce,
            'attachment_id' : $(this).attr('id')
        };

        $(this).hide(); // Hide the "Optimize Now" button
        $(this).after(ajax_object.loaderImageHtml); // Show the loader image

        $.post(ajax_object.ajax_url, data, function (responseData, status)
        {
            // Search the hidden "Optimize Now" button by id, than get the parent "div" tag and replaced with statistics
            $("p[id=" + responseData.id + "]").parent().after(responseData.html).remove();
        });
    });
});