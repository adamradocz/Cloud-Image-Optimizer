<?php
namespace InnovationX\ImageOptimizer;

class ImageManager
{
    private $_imageMetaKey;
    private $_optimizer;

    public function __construct($configuration, $optimizer)
    {
        $this->_imageMetaKey = $configuration['image_meta_key'];
        $options = get_option($configuration['option_name']);
        $this->_optimizer = $optimizer;

        if ($options['is_automatically_optimize_on_upload'])
        {
            // Fires on image upload/edit
            add_filter('wp_update_attachment_metadata', array($this, 'AutomaticallyOptimizeUpdatedImage'), 10, 2);
        }

        // Fires on attachment delete
        add_filter('delete_attachment', array($this, 'DeleteConvertedImages'), 10, 1);
    }

    function AutomaticallyOptimizeUpdatedImage($metadata, $attachmentId)
    {
        if (!wp_attachment_is_image($attachmentId))
            return $metadata;
        
        $this->_optimizer->OptimizeImage($metadata, $attachmentId);
        return $metadata;
    }

    /**
     * Delete the converted images of an attachment
     * 
     * @param $attachmentId
     * @return mixed - $attachmentId
     */
    function DeleteConvertedImages($attachmentId)
    {
        if (!wp_attachment_is_image($attachmentId))
            return $attachmentId;

        $iioMetadata = get_post_meta($attachmentId, $this->_imageMetaKey, true);
        if (empty($iioMetadata))
            return $attachmentId;

        $imageFilePath = get_attached_file($attachmentId);
        $imageFolderPath = trailingslashit(dirname($imageFilePath));

        foreach ($iioMetadata['sizes'] as $imageSizeMetadata)
        {
            foreach ($imageSizeMetadata['converted_images'] as $convertedImage)
            {
                $imageFilePath = $imageFolderPath . $convertedImage['name'];
                if (is_writable($imageFilePath))
                    unlink($imageFilePath);
            }
        }

        return $attachmentId;
    }

}