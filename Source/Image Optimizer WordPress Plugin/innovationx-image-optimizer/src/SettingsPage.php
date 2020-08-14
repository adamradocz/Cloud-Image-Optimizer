<?php

namespace InnovationX\ImageOptimizer;

class SettingsPage
{
    private $_id;
    private $_name;
    private $_shortName;
    private $_version;
    private $_optionName;
    private $_pricingPlansUrl;

    private $_options;
    private $_permissionManager;

    private $_adminPageSlug;
    private $_optionsGroup;
    private $_settingsSectionCloudId;
    private $_settingsSectionBasicId;
    private $_settingsSectionProfessionalId;

    public function __construct($configuration, $permissionManager)
    {
        $this->_id = $configuration['id'];
        $this->_name = $configuration['name'];
        $this->_shortName = $configuration['short_name'];
        $this->_version = $configuration['version'];
        $this->_optionName = $configuration['option_name'];
        $this->_pricingPlansUrl = $configuration['pricing_plans_url'];

        $this->_options = get_option($configuration['option_name']);
        $this->_permissionManager = $permissionManager;

        $this->_adminPageSlug = $configuration['id'];
        $this->_optionsGroup = 'innovationx_io_options_group';
        $this->_settingsSectionCloudId = $this->_adminPageSlug . '-cloud';
        $this->_settingsSectionBasicId = $this->_adminPageSlug . '-basic';
        $this->_settingsSectionProfessionalId = $this->_adminPageSlug . '-pro';

        // Add admin menu
        add_action( 'admin_menu', array($this, 'AddAdminMenu') );

        // Initialize the settings form
        add_action('admin_init', array($this, 'SettingsFormInit'));

        // Enqueue Settings Admin scripts
        add_action('admin_enqueue_scripts', array($this, 'EnqueueSettingsAdminScripts'));

        //Handle Optimize Now button click (Ajax call)
        add_action('wp_ajax_ValidateApiKey', array($this, 'ValidateApiKey'));
    }
    
    function AddAdminMenu()
    {
        add_menu_page('InnovationX Dashboard', $this->_shortName, 'manage_options', $this->_id, array($this, 'OptionsPage'), 'dashicons-lightbulb', 81);
    }

    /**
     * Add options page. add_options_menu callback
     */
    function OptionsPage()
    {
        if (!current_user_can('manage_options'))
            wp_die('You do not have sufficient permissions to access this page.');

        require_once(plugin_dir_path(dirname(__FILE__)) . 'views/SettingsPage/OptionsPageView.php');
    }

    /**
     * Register and add settings
     */
    function SettingsFormInit()
    {
        // Cloud section
        register_setting(
            $this->_optionsGroup,    // A settings group name. Must exist prior to the register_setting call. This must match the group name in settings_fields()
            $this->_optionName,  // A settings group name. Must exist prior to the register_setting call. This must match the group name in settings_fields()
            array($this, 'Sanitize'));  // A callback function that sanitizes the option's value.

        add_settings_section(
            $this->_settingsSectionCloudId,  // String for use in the 'id' attribute of tags.
            'Cloud settings',    // Title of the section.
            array($this, 'DisplaySectionCloudInfo'),    // Callback function that fills the section with the desired content. The function should echo its output.
            $this->_adminPageSlug);  // The menu page on which to display this section. Should match $menu_slug from Function Reference/add theme page

        add_settings_field(
            'api_key', // String for use in the 'id' attribute of tags.
            'API Key', // Title of the field.
            array($this, 'ApiKeyCallback'), // Callback function that fills the field with the desired inputs as part of the larger form. Passed a single argument, the $args array. Name and id of the input should match the $id given to this function. The function should echo its output.
            $this->_adminPageSlug, // The menu page on which to display this field. Should match $menu_slug from add_theme_page() or from do_settings_sections().
            $this->_settingsSectionCloudId // The section of the settings page in which to show the box (default or a section you added with add_settings_section()).
        //array('label_for' => 'myprefix_setting-id') // Additional arguments that are passed to the $callback function. The 'label_for' key/value pair can be used to format the field title like so: <label for="value">$title</label>.
        );

        add_settings_field('api_key_status', 'API Key Status', array($this, 'ApiKeyStatusCallback'), $this->_adminPageSlug, $this->_settingsSectionCloudId);

        if (!$this->HasApiKey())
            return;

        // Basic section
        register_setting($this->_optionsGroup, $this->_optionName, array($this, 'Sanitize'));
        add_settings_section($this->_settingsSectionBasicId, 'Basic settings', array($this, 'DisplaySectionBasicInfo'), $this->_adminPageSlug);
        add_settings_field('is_automatically_optimize_on_upload', 'Auto optimize', array($this, 'IsAutomaticallyOptimizeOnUploadCallback'), $this->_adminPageSlug, $this->_settingsSectionBasicId);
        add_settings_field('is_lossless', 'Optimization Mode', array($this, 'IsLosslessCallback'), $this->_adminPageSlug, $this->_settingsSectionBasicId);
        add_settings_field('is_optimize_original', 'Original Image', array($this, 'IsOptimizeOriginalCallback'), $this->_adminPageSlug, $this->_settingsSectionBasicId);
        add_settings_field('is_optimize_original_only_lossless', '', array($this, 'IsOptimizeOriginalOnlyLosslessCallback'), $this->_adminPageSlug, $this->_settingsSectionBasicId);

        // Professional section
        register_setting($this->_optionsGroup, $this->_optionName, array($this, 'Sanitize'));
        add_settings_section($this->_settingsSectionProfessionalId, 'Professional settings', array($this, 'DisplaySectionProfessionalInfo'), $this->_adminPageSlug);
        add_settings_field('is_convert', 'Convert', array($this, 'IsConvertCallback'), $this->_adminPageSlug, $this->_settingsSectionProfessionalId);
        //add_settings_field('is_convert_to_webp', '', array($this, 'IsConvertToWebpCallback'), $this->_adminPageSlug, $this->_settingsSectionProfessionalId);
        //add_settings_field('is_convert_to_jxr', '', array($this, 'IsConvertToJxrCallback'), $this->_adminPageSlug, $this->_settingsSectionProfessionalId);
        //add_settings_field('is_convert_to_apng', '', array($this, 'IsConvertToApngCallback'), $this->_adminPageSlug, $this->_settingsSectionProfessionalId);
        add_settings_field('is_browser_compatibility_enabled', 'Compatibility mode', array($this, 'IsBrowserCompatibilityEnabledCallback'), $this->_adminPageSlug, $this->_settingsSectionProfessionalId);
    }

    /**
     * Sanitize each setting field as needed.
     *
     * @param array $input - Contains all the setted settings fields as array keys.
     *
     * isset() is significantly faster than (bool) casting, especially when $input doesn't setted.
     * @return option array
     */
    function Sanitize($input)
    {
        if ($this->_options['api_key'] !== $input['api_key'])
        {
            $this->_options['api_key'] = isset($input['api_key']) === true ? sanitize_text_field(trim($input['api_key'], " \t\n\r\0\x0B")) : '';
            $permissions = $this->_permissionManager->ReceiveUserPermissions($this->_options['api_key']);
            if ($permissions['is_success'] === true)
            {
                $this->_options['permissions'] = $permissions['permissions'];
            }
            else
            {
                $this->_options['permissions'] = null;
                add_settings_error('API Key Status', 'api_key_status', $permissions['message'], 'error');
            }
        }

        $this->_options['is_automatically_optimize_on_upload'] = isset($input['is_automatically_optimize_on_upload']) === true ? true : false;
        $this->_options['is_lossless'] = (bool)$input['is_lossless'] === true ? true : false;    // Radioboxes are always setted
        $this->_options['is_optimize_original'] = isset($input['is_optimize_original']) === true ? true : false;
        $this->_options['is_optimize_original_only_lossless'] = isset($input['is_optimize_original_only_lossless']) === true ? true : false;
        $this->_options['is_convert'] = isset($input['is_convert']) === true ? true : false;
        $this->_options['is_convert_to_webp'] = isset($input['is_convert_to_webp']) === true ? true : false;
        $this->_options['is_convert_to_jxr'] = isset($input['is_convert_to_jxr']) === true ? true : false;
        $this->_options['is_convert_to_apng'] = isset($input['is_convert_to_apng']) === true ? true : false;
        $this->_options['is_browser_compatibility_enabled'] = isset($input['is_browser_compatibility_enabled']) === true ? true : false;

        return $this->_options;
    }

    /**
     * #### Cloud section ####
     */
    function DisplaySectionCloudInfo()
    {
        //print 'Cloud settings';
    }

    function ApiKeyCallback()
    {
        $api_key = esc_attr($this->_options['api_key']);

        printf('<div><input type="text" id="api_key" name="%s[api_key]" value="%s" class="regular-text" /><p class="button button-primary" id="iio-validate-api-key" >Validate</p></div>',
               esc_attr($this->_optionName),
               $api_key
        );

        if ($this->HasApiKey())
            return;
        ?>
        <p>Don't have an API Key yet? <a href="<?php echo esc_url($this->_pricingPlansUrl, array('http', 'https')); ?>">Click
                here to create one, it's FREE.</a></p>
        <?php
    }

    function ValidateApiKey()
    {
        // Verifies the AJAX request to prevent processing requests external of the blog.
        check_ajax_referer('ValidateApiKey', 'nonce');

        if (!current_user_can('manage_options'))
            wp_die('You do not have sufficient permissions to work with InnovationX Image Optimizer\'s settings.');

        $html = '';
        $permissions = $this->_permissionManager->ReceiveUserPermissions($this->_options['api_key']);
        if ($permissions['is_success'] === true)
        {
            $this->_options['permissions'] = $permissions['permissions'];
            update_option($this->_optionName, $this->_options);
            $html = '<p id="iio-api-key-status" style="color:green;"><b>' . esc_html($permissions['permissions']['role_name']) . '</b></p>';
        }
        else
        {
            $html = '<p id="iio-api-key-status" style="color:red;"><b>' . esc_html($permissions['message']) . '</b></p>';
        }

        $responseData = array(
            'html' => $html
        );

        // Send a JSON response back to an AJAX request, and die().
        wp_send_json($responseData);
    }

    function ApiKeyStatusCallback()
    {
        if ($this->HasApiKey())
        {
            printf('<p id="iio-api-key-status" style="color:green;"><b>%s</b></p>', esc_html($this->_options['permissions']['role_name']));
        }
        else
        {
            printf('<p id="iio-api-key-status" style="color:Red;"><b>Invalid API Key.</b></p>');
        }
    }

    /**
     * #### Basic section ####
     */
    function DisplaySectionBasicInfo()
    {
        //print 'Basic settings';
    }

    function IsAutomaticallyOptimizeOnUploadCallback()
    {
        $is_automatically_optimize_on_upload = $this->_options['is_automatically_optimize_on_upload'];
        ?>
        <fieldset>
            <legend class="screen-reader-text"><span></span></legend>
            <label for="<?php echo esc_attr($this->_optionName); ?>[is_automatically_optimize_on_upload]">
                <input type="checkbox" id="is_automatically_optimize_on_upload"
                       name="<?php echo esc_attr($this->_optionName); ?>[is_automatically_optimize_on_upload]"
                       value="1" <?php checked($is_automatically_optimize_on_upload, true); ?> />
                <span>Automatically optimize images on upload</span>
            </label>
        </fieldset>
        <?php
    }

    function IsLosslessCallback()
    {
        $is_lossless = $this->_options['is_lossless'];

        ?>
        <fieldset>
            <legend class="screen-reader-text"><span>input type="radio"</span></legend>
            <p>
                <input type="radio" id="is_lossless" name="<?php echo esc_attr($this->_optionName); ?>[is_lossless]"
                       value="0" <?php checked($is_lossless, false); ?> />
                <span>Lossy</span>
            <p class="settings-info"><b>Lossy optimization:</b> lossy has much better compression rate than lossless
                compression.</br> The resulting image is NOT 100% identical with the original, but quality loss is
                minimal.</p>
            </p>
            <p>
                <input type="radio" id="is_lossless" name="<?php echo esc_attr($this->_optionName); ?>[is_lossless]"
                       value="1" <?php checked($is_lossless, true); ?> />
                <span>Lossless</span>
            <p class="settings-info"><b>Lossless optimization:</b> the optimized image will be identical with the
                original.</p>
            </p>
        </fieldset>
        <?php
    }

    function IsOptimizeOriginalCallback()
    {
        $is_optimize_original = $this->_options['is_optimize_original'];

        ?>
        <fieldset>
            <legend class="screen-reader-text"><span></span></legend>
            <label for="<?php echo esc_attr($this->_optionName); ?>[is_optimize_original]">
                <input type="checkbox" id="is_optimize_original"
                       name="<?php echo esc_attr($this->_optionName); ?>[is_optimize_original]"
                       value="1" <?php checked($is_optimize_original, true); ?> />
                <span>Optimize original image</span>
            </label>
        </fieldset>
        <?php
    }

    function IsOptimizeOriginalOnlyLosslessCallback()
    {
        $is_optimize_original_only_lossless = $this->_options['is_optimize_original_only_lossless'];

        ?>
        <fieldset>
            <legend class="screen-reader-text"><span></span></legend>
            <label for="<?php echo esc_attr($this->_optionName); ?>[is_optimize_original_only_lossless]">
                <input type="checkbox" id="is_optimize_original_only_lossless"
                       name="<?php echo esc_attr($this->_optionName); ?>[is_optimize_original_only_lossless]"
                       value="1" <?php checked($is_optimize_original_only_lossless, true); ?> />
                <span>Optimize original image only lossless</span>
            </label>
        </fieldset>
        <?php
    }

    /**
     * #### Professional section ####
     */
    function DisplaySectionProfessionalInfo()
    {
        //print 'Professional settings';
    }

    function IsConvertCallback()
    {
        $isConvert = $this->_options['is_convert'];
        ?>
        <fieldset>
            <legend class="screen-reader-text"><span></span></legend>
            <p>
                <input type="checkbox" id="is_convert" name="<?php echo esc_attr($this->_optionName); ?>[is_convert]"
                       value="1" <?php checked($isConvert, true);
                $this->Disabled(); ?> />
                <span>Save extra ~30% by converting images to newer file formats (WebP, Jpeg-XR, Apng)</span>
            <p class="settings-info">Original images are never deleted.</p>
            </p>
        </fieldset>
        <?php
    }

    function IsConvertToWebpCallback()
    {
        $isConvertToWebp = $this->_options['is_convert_to_webp'];
        ?>
        <fieldset>
            <legend class="screen-reader-text"><span></span></legend>
            <p>
                <input type="checkbox" id="is_convert_to_webp"
                       name="<?php echo esc_attr($this->_optionName); ?>[is_convert_to_webp]"
                       value="1" <?php checked($isConvertToWebp, true);
                $this->Disabled(); ?> />
                <span>To WebP</span>
            <p class="settings-info">WebP images are supported in Chrome, Android Browser, Opera.</p>
            </p>
        </fieldset>
        <?php
    }

    function IsConvertToJxrCallback()
    {
        $isConvertToJxr = $this->_options['is_convert_to_jxr'];
        ?>
        <fieldset>
            <legend class="screen-reader-text"><span></span></legend>
            <p>
                <input type="checkbox" id="is_convert_to_jxr"
                       name="<?php echo esc_attr($this->_optionName); ?>[is_convert_to_jxr]"
                       value="1" <?php checked($isConvertToJxr, true);
                $this->Disabled(); ?> />
                <span>To Jpeg-XR</span>
            <p class="settings-info">Jpeg-XR images are supported supported in Internet Explorer, Edge.</p>
            </p>
        </fieldset>
        <?php
    }

    function IsConvertToApngCallback()
    {
        $isConvertToApng = $this->_options['is_convert_to_apng'];
        ?>
        <fieldset>
            <legend class="screen-reader-text"><span></span></legend>
            <p>
                <input type="checkbox" id="is_convert_to_apng"
                       name="<?php echo esc_attr($this->_optionName); ?>[is_convert_to_apng]"
                       value="1" <?php checked($isConvertToApng, true);
                $this->Disabled(); ?> />
                <span>To APNG</span>
            <p class="settings-info">APNG images are supported in Firefox, Safari.</p>
            </p>
        </fieldset>
        <?php
    }

    function IsBrowserCompatibilityEnabledCallback()
    {
        $isBrowserCompatibilityEnabled = $this->_options['is_browser_compatibility_enabled'];
        ?>
        <fieldset>
            <legend class="screen-reader-text"><span></span></legend>
            <p>
                <input type="checkbox" id="is_browser_compatibility_enabled"
                       name="<?php echo esc_attr($this->_optionName); ?>[is_browser_compatibility_enabled]"
                       value="1" <?php checked($isBrowserCompatibilityEnabled, true);
                $this->Disabled(); ?> />
                <span>Enable browser compatibility for older browsers to serve converted images (Android Browser 4.3-4.4.4, Internet Explorer 11, Opera Mini 8).</span>
            <p class="settings-info">Your website load time will be slightly slower.</p>
            </p>
        </fieldset>
        <?php
    }

    function EnqueueSettingsAdminScripts($hook)
    {
        // Only applies to Upload panel
        if ($hook !== 'settings_page_' . $this->_id)
            return;

        $scriptName = $this->_options['is_debug'] === true ? 'admin-settings.js' : 'admin-settings.min.js';
        wp_enqueue_script($this->_id . 'admin-settings', plugins_url('/js/' . $scriptName, dirname(__FILE__)), array('jquery'), $this->_version);

        // in JavaScript, object properties are accessed as ajax_object.ajax_url, ajax_object.nonce
        wp_localize_script($this->_id . 'admin-settings', 'ajax_object', array(
                                                           'ajax_url'        => admin_url('admin-ajax.php'),
                                                           'nonce'           => wp_create_nonce('ValidateApiKey'),
                                                           'loaderImageHtml' => '<img src="' . plugins_url('/images/loading.gif', dirname(__FILE__)) . '" id="iio-loading-gif" >'
                                                       )
        );
    }

    /* *
     * ### Helpers ###
     */

    private function HasApiKey()
    {
        return ($this->_options['permissions'] !== null) ? true : false;
    }

    /**
     * Print 'disabled' HTML element or ''
     */
    private function Disabled()
    {
        echo $this->HasApiKey() ? '' : 'disabled';
    }

}
