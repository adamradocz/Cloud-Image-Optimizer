<?php

namespace InnovationX\ImageOptimizer;

class ContentImageHandler
{
    private $_imageMetaKey;

    public function __construct($configuration)
    {
        $options = get_option($configuration['option_name']);
        $this->_imageMetaKey = $configuration['image_meta_key'];

        if ($options['is_browser_compatibility_enabled'])
        {
            // Enqueue Picturefill script with Type support plugin (JPG2000, APNG and JPEG XR)
            add_action('wp_enqueue_scripts', array($this, 'EnqueuePicturefillScript'), 10);

            // Make scripts async
            add_filter('script_loader_tag', array($this, 'MakeScriptsAsync'), 10, 2);
        }

        // Fires before showing the content
        add_filter('the_content', array($this, 'MakeContentImagesFormatBased'), 10, 1);
    }

    function EnqueuePicturefillScript($hook)
    {
        wp_enqueue_script('picturefill-with-type-support', plugins_url('/js/picturefill.min.js', dirname(__FILE__) ), array('jquery'), '3.0.2', false);
        wp_add_inline_script( 'picturefill-with-type-support', 'document.createElement("picture");', 'before');
    }

    function MakeScriptsAsync($tag, $handle)
    {
        // The array of the async scripts
        $scriptsToAsync = array('picturefill.min.js');
        foreach($scriptsToAsync as $asyncScript)
        {
            if(strpos($tag, $asyncScript ) !== false)
                return str_replace(' src', ' async src', $tag);
        }

        return $tag;
    }

    /**
     * Description: Filters ‘img’ elements in post content to add ‘picture’, ‘source’ tag and 'type' attribute.
     *
     * @param $content - Post content to be filtered.
     * @return mixed - Modified post content with ‘picture’, ‘source’ tag and 'type' attribute added to images.
     */
    function MakeContentImagesFormatBased($content)
    {
        // Find images in content and return if not found
        if (!preg_match_all('/<img [^>]+>/', $content, $matches))
            return $content;

        $selectedImages = array();
        foreach ($matches[0] as $imageTag)
        {
            if (preg_match('/wp-image-([0-9]+)/i', $imageTag, $classId))
            {
                $attachmentId = absint($classId[1]);
                $selectedImages[$imageTag] = $attachmentId;
            }
        }

        foreach ($selectedImages as $imageTag => $attachmentId )
        {
            $pictureTag = $this->GenerateFormatBasedImage($imageTag, $attachmentId);
            if ($pictureTag !== false) // Must identical comparison
                $content = str_replace($imageTag, $pictureTag, $content);
        }

        return $content;
    }

    function GenerateFormatBasedImage($imageTag, $attachmentId)
    {
        $iioMetadata = get_post_meta($attachmentId, $this->_imageMetaKey, true);
        if (empty($iioMetadata))
            return false;

        if (!preg_match('/srcset="([^"]+)"/', $imageTag, $srcset))
            return false;

        if (!preg_match('/sizes="([^"]+)"/', $imageTag, $sizes))
            return false;

        $srcsetFormats = array();
        foreach ($iioMetadata['sizes'] as $image)
        {
            // Check whether 'srcset' contain optimized image which has a converted version
            if (strpos($srcset[1], $image['name']) !== false && $image['is_converted'])
            {
                foreach ($image['converted_images'] as $fileType => $imageData)
                {
                    if (!array_key_exists($fileType, $srcsetFormats))
                        $srcsetFormats[$fileType] = str_replace($image['name'], $imageData['name'], $srcset[1]);
                    else
                        $srcsetFormats[$fileType] = str_replace($image['name'], $imageData['name'], $srcsetFormats[$fileType]);
                }
            }
        }

        if (empty($srcsetFormats))
            return false;

        // Generate 'picture' tag
        $pictureTag = '<picture>';

        // The format order is very important
        if (array_key_exists('image/webp', $srcsetFormats))
            $pictureTag .= '<source srcset="' . $srcsetFormats['image/webp'] . '" sizes="' . $sizes[1] . '" type="image/webp">';

        if (array_key_exists('image/vnd.ms-photo', $srcsetFormats))
            $pictureTag .= '<source srcset="' . $srcsetFormats['image/vnd.ms-photo'] . '" sizes="' . $sizes[1] . '" type="image/vnd.ms-photo">';

        if (array_key_exists('image/apng', $srcsetFormats))
            $pictureTag .= '<source srcset="' . $srcsetFormats['image/apng'] . '" sizes="' . $sizes[1] . '" type="image/apng">';

        $pictureTag .= str_replace('/>', '>', $imageTag);
        $pictureTag .= '</picture>';

        return $pictureTag;
    }
}