<?php

namespace InnovationX\ImageOptimizer;

class Optimizer
{
    private $_options;
    private $_imageMetaKey;
    private $_user;

    public function __construct($configuration, $user)
    {
        $this->_options = get_option($configuration['option_name']);
        $this->_imageMetaKey = $configuration['image_meta_key'];
        $this->_user = $user;
    }

    /**
     * @param $metadata
     * @param $attachmentId
     * @return $metadata - Image metadata for WordPress hook
     */
    public function OptimizeImage($metadata, $attachmentId)
    {        
        if (!$this->_user->CanOptimizeOnCloud($attachmentId))
            return $metadata;

        $imageFilePath = get_attached_file($attachmentId);
        $fileType = get_post_mime_type($attachmentId);

        $postData = array(
            'api_key'         => $this->_options['api_key'],
            'is_lossless'     => $this->_options['is_lossless'],
            'is_convert'      => $this->_options['is_convert'],
            'image_file_path' => $imageFilePath,
            'file_type'       => $fileType
        );

        //$optimizedImage = array();
        $iioMetadata = array();

        // Optimize original image
        if ($this->_options['is_optimize_original'])
        {
            if ($this->_options['is_optimize_original_only_lossless'])
            {
                $postData['is_lossless'] = true;
                $optimizedImage = $this->PostOptimizationRequest($postData);
            }
            else
            {
                $optimizedImage = $this->PostOptimizationRequest($postData);
            }

            if ($optimizedImage === false)
                return $metadata;

            $saveResult = $this->SaveOptimizedImage($optimizedImage, $imageFilePath);
            if (!$saveResult)
                return $metadata;

            $iioMetadata['sizes']['full'] = $this->GenerateCustomMetadata($optimizedImage);
        }

        // Optimize image sizes
        $postData['is_lossless'] = $this->_options['is_lossless'];
        $imageFolderPath = trailingslashit(dirname($imageFilePath));
        foreach ($metadata['sizes'] as $sizeName => $sizeData)
        {
            $imageFilePath = $imageFolderPath . $sizeData['file'];
            if (!file_exists($imageFilePath))
                return $metadata;

            $postData['image_file_path'] = $imageFilePath;

            $optimizedImage = $this->PostOptimizationRequest($postData);
            if ($optimizedImage === false)
                return $metadata;

            $saveResult = $this->SaveOptimizedImage($optimizedImage, $imageFilePath);
            if (!$saveResult)
                return $metadata;

            $iioMetadata['sizes'][$sizeName] = $this->GenerateCustomMetadata($optimizedImage);
        }

        // Add or Update custom metadata
        $iioMetadata = $this->GenerateTotalMetadata($iioMetadata);
        update_post_meta($attachmentId, $this->_imageMetaKey, $iioMetadata);

        return $metadata;
    }

    /**
     * @param $postData
     * @return array|bool - On success return an array contains an optimized image with data from the server, else return false
     */
    function PostOptimizationRequest($postData)
    {
        $boundary = '---011000010111000001101001'; // Just a random string
        $headers = array(
            'content-type' => 'multipart/form-data; boundary=' . $boundary
        );

        $bodyData = array(
            'ApiKey'     => $postData['api_key'],
            'IsConvert'  => $postData['is_convert'] ? 'true' : 'false',
            'IsLossless' => $postData['is_lossless'] ? 'true' : 'false'
        );

        // First, add the standard POST fields:
        $body = '';
        foreach ($bodyData as $name => $value)
        {
            $body .= '--' . $boundary . "\r\n";
            $body .= 'Content-Disposition: form-data; name="' . $name . '"' . "\r\n\r\n";
            $body .= $value . "\r\n";
        }

        // Add the file field
        $body .= '--' . $boundary . "\r\n";
        $body .= 'Content-Disposition: form-data; name="' . 'image1' . '"; filename="' . basename($postData['image_file_path']) . '"' . "\r\n";
        $body .= 'Content-Type: ' . $postData['file_type'] . "\r\n\r\n\r\n";
        $body .= file_get_contents($postData['image_file_path']);
        $body .= "\r\n";

        // Add the closing boundary
        $body .= '--' . $boundary . '--';

        // Send request
        $response = wp_remote_post('http://localhost:5000/api/ImageOptimizer', array(
            'httpversion' => '1.1',
            'blocking'    => true,
            'compress'    => false,
            'timeout'     => 180,
            'sslverify'   => false,
            'headers'     => $headers,
            'body'        => $body
        ));

        // Handle errors
        if (wp_remote_retrieve_response_code($response) !== 200)
        {
            if (is_wp_error($response))
            {
                $responseErrorMessage = $response->get_error_message();
            }

            $responseBody = wp_remote_retrieve_body($response);
            if (!empty($responseBody))
            {
                $optimizedImage = json_decode($responseBody, true);
                $responseErrorMessage = $optimizedImage[0]['Message'];
            }

            $responseErrorMessage = 'Unknown error occurred';

            return false;
        }

        // Return optimized image
        $responseBody = wp_remote_retrieve_body($response);
        $optimizedImage = json_decode($responseBody, true);
        return $optimizedImage[0];
    }

    /**
     * @param $optimizedImage - Optimized image data from the server
     * @param $imageFilePath - Original image file path
     * @return bool - If saving was successful than true, else false
     */
    function SaveOptimizedImage($optimizedImage, $imageFilePath)
    {
        // Save the image
        $image = base64_decode($optimizedImage['Image']);
        $saveResult = file_put_contents($imageFilePath, $image);
        if (!$saveResult)
            return false;

        // Save the converted images
        if (!is_null($optimizedImage['ConvertedImages']))
        {
            foreach ($optimizedImage['ConvertedImages'] as $convertedImage)
            {
                $image = base64_decode($convertedImage['Image']);
                $saveResult = file_put_contents($imageFilePath . $convertedImage['FileExtension'], $image);
                if (!$saveResult)
                    return false;
            }
        }

        return true;
    }

    /**
     * @param $optimizedImage - Optimized image data from the server
     * @return array - Innovationx Image Optimizer custom metadata
     */
    function GenerateCustomMetadata($optimizedImage)
    {
        $singleImageMetadata = array(
            'name'               => $optimizedImage['Name'],
            'original_size'      => $optimizedImage['OriginalSize'],
            'optimized_size'     => $optimizedImage['OptimizedSize'],
            'savings'            => $optimizedImage['Savings'],
            'savings_percent'    => $optimizedImage['SavingsInPercentage'],
            'is_lossless'        => $optimizedImage['IsLossless'],
            'is_converted'       => $optimizedImage['IsConvert'],
            'has_webp'           => false,
            'has_jxr'            => false,
            'has_apng'           => false,
            'optimization_level' => $optimizedImage['OptimizationLevel'],
            'uploaded'           => $optimizedImage['Uploaded'],
            'optimization_time'  => $optimizedImage['OptimizationTime'],
            'converted_images'   => null
        );

        if (!is_null($optimizedImage['ConvertedImages']))
        {
            foreach ($optimizedImage['ConvertedImages'] as $convertedImage)
            {
                $fileType = $convertedImage['FileType'];
                if ($fileType === 'image/webp')
                {
                    $singleImageMetadata['has_webp'] = true;
                }
                else if ($fileType === 'image/vnd.ms-photo' || $fileType === 'image/jxr')
                {
                    $singleImageMetadata['has_jxr'] = true;
                }
                else if ($fileType === 'image/apng')
                {
                    $singleImageMetadata['has_apng'] = true;
                }

                $singleImageMetadata['converted_images'][$fileType] = array(
                    'name'               => $convertedImage['Name'],
                    'original_size'      => $convertedImage['OriginalSize'],
                    'optimized_size'     => $convertedImage['OptimizedSize'],
                    'savings'            => $convertedImage['Savings'],
                    'savings_percent'    => $convertedImage['SavingsInPercentage'],
                    'is_lossless'        => $convertedImage['IsLossless'],
                    'optimization_level' => $convertedImage['OptimizationLevel'],
                    'uploaded'           => $convertedImage['Uploaded'],
                    'optimization_time'  => $convertedImage['OptimizationTime']
                );
            }
        }

        return $singleImageMetadata;
    }

    /**
     * @param $iioMetadata - Generated Innovationx Image Optimizer custom metadata
     * @return mixed - Generated Innovationx Image Optimizer custom metadata with total fields
     */
    function GenerateTotalMetadata($iioMetadata)
    {
        $iioMetadata['total_original_size'] = 0;
        $iioMetadata['total_optimized_size'] = 0;
        $iioMetadata['total_savings'] = 0;
        $iioMetadata['extra_total_webp_savings'] = 0;
        $iioMetadata['extra_total_jxr_savings'] = 0;
        $iioMetadata['extra_total_apng_savings'] = 0;
        $iioMetadata['extra_total_converted_savings'] = 0;

        foreach ($iioMetadata['sizes'] as $size)
        {
            $iioMetadata['total_original_size'] += $size['original_size'];
            $iioMetadata['total_optimized_size'] += $size['optimized_size'];
            $iioMetadata['total_savings'] += $size['savings'];

            if (!is_null($size['converted_images']))
            {
                foreach ($size['converted_images'] as $convertedImageName => $convertedImageData)
                {
                    $iioMetadata['extra_total_converted_savings'] += $convertedImageData['savings'];

                    if ($convertedImageName === 'image/webp')
                    {
                        $iioMetadata['extra_total_webp_savings'] += $convertedImageData['savings'];
                    }
                    else if ($convertedImageName === 'image/vnd.ms-photo' || $convertedImageName === 'image/jxr')
                    {
                        $iioMetadata['extra_total_jxr_savings'] += $convertedImageData['savings'];
                    }
                    else if ($convertedImageName === 'image/apng')
                    {
                        $iioMetadata['extra_total_apng_savings'] += $convertedImageData['savings'];
                    }
                }
            }
        }

        $iioMetadata['total_savings_percent'] = $iioMetadata['total_savings'] / $iioMetadata['total_original_size'];
        return $iioMetadata;
    }

}
