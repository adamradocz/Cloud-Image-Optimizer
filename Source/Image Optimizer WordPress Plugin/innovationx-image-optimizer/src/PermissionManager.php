<?php

namespace InnovationX\ImageOptimizer;

class PermissionManager
{
    /**
     * @param $apiKey
     * @return array|bool - On success return the user's permissions formatted as 'options' format, else false
     */
    public function ReceiveUserPermissions($apiKey)
    {
        $headers = array('apiKey' => $apiKey);

        // Send request
        $response = wp_remote_get('http://localhost:5000/api/permission',
                                  array(
                                      'httpversion' => '1.1',
                                      'blocking'    => true,
                                      'compress'    => false,
                                      'timeout'     => 20,
                                      'sslverify'   => false,
                                      'headers'     => $headers,
                                      'body'        => array()
                                  ));

        $permissions = array();

        // Handle errors
        if (wp_remote_retrieve_response_code($response) !== 200)
        {
            $responseBody = wp_remote_retrieve_body($response);
            if (!empty($responseBody))
            {
                $responseError = json_decode($responseBody, true);
                $responseErrorMessage = $responseError['Message'];
            }
            else if (is_wp_error($response))
            {
                $responseErrorMessage = $response->get_error_message();
            }
            else
            {
                $responseErrorMessage = 'Unknown error occurred';
            }

            $permissions['is_success'] = false;
            $permissions['message'] = $responseErrorMessage;
            return $permissions;
        }

        // Return user's permissions
        $responseBody = wp_remote_retrieve_body($response);
        $responsePermissions = json_decode($responseBody, true);
        $permissions['is_success'] = true;
        $permissions['permissions'] = $this->ConvertResponsePermissionsToOptionFormat($responsePermissions);
        return $permissions;
    }

    /**
     * @param $responsePermissions - Permissions array gets from the server as response
     * @return array - Permissions array formatted as 'options' format
     */
    private function ConvertResponsePermissionsToOptionFormat($responsePermissions)
    {
        $permissions = array(
            'role_name'               => $responsePermissions['RoleName'],
            'allowed_file_types'      => $responsePermissions['AllowedFileTypes'],
            'allowed_file_extensions' => $responsePermissions['AllowedFileExtensions'],
            'allowed_image_size'      => $responsePermissions['AllowedImageSize'],
            'image_limit_per_month'   => $responsePermissions['ImageLimitPerMonth'],
            'can_optimize_lossy'      => $responsePermissions['CanOptimizeLossy'],
            'can_convert_to_webp'     => $responsePermissions['CanConvertToWebp'],
            'can_convert_to_jxr'      => $responsePermissions['CanConvertToJxr'],
            'can_convert_to_apng'     => $responsePermissions['CanConvertToApng'],
            'optimization_level'      => $responsePermissions['OptimizationLevel']
        );

        return $permissions;
    }
}