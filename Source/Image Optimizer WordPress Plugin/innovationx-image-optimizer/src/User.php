<?php

namespace InnovationX\ImageOptimizer;

class User
{
    private $_options;

    public function __construct($configuration)
    {
        $this->_options = get_option($configuration['option_name']);
    }

    /**
     * @param $attachmentId - Image's attachment ID.
     * @return bool
     */
    public function CanOptimizeOnCloud($attachmentId)
    {
        // Check the cloud permission
        // If the user hasn't a valid API key, than the permission is null
        if ($this->_options['permissions'] === null)
            return false;

        // Check the file type
        $allowedFileTypes = $this->_options['permissions']['allowed_file_types'];
        $fileType = get_post_mime_type($attachmentId);
        if (strpos($allowedFileTypes, $fileType) === false)
            return false;

        // Check the image size
        $allowedFileSize = $this->_options['permissions']['allowed_image_size'];
        $imageFilePath = get_attached_file($attachmentId);
        if (!file_exists($imageFilePath))
            return false;

        if (filesize($imageFilePath) > $allowedFileSize)
            return false;

        return true;
    }
}