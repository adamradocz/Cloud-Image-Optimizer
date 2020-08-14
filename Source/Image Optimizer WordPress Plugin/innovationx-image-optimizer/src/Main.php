<?php

namespace InnovationX\ImageOptimizer;

class Main
{
    private $_options;

    public function __construct($configuration)
    {
        $this->_options = get_option($configuration['option_name']);

        if (is_admin())
        {
            $user = new User($configuration);
            $optimizer = new Optimizer($configuration, $user);
            $permissionManager = new PermissionManager();
            new SettingsPage($configuration, $permissionManager);
            new MediaPage($configuration, $optimizer, $user);
            new ImageManager($configuration, $optimizer);
        }
        else if ($this->CanUseConvertedImages())
        {
            new ContentImageHandler($configuration);
        }
    }

    /* *
     * ### Helpers ###
     */
    private function CanUseConvertedImages()
    {
        if ($this->_options['permissions'] === null)
            return false;
        
        if ($this->_options['permissions']['can_convert_to_webp'] ||
            $this->_options['permissions']['can_convert_to_jxr'] ||
            $this->_options['permissions']['can_convert_to_apng']
        )
            return true;
        
        return false;
    }
}
