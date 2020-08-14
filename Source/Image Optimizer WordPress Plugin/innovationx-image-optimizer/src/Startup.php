<?php

namespace InnovationX\ImageOptimizer;

class Startup
{
    private $_options;
    private $_transient_id_activate_welcome_page = 'iio_activate_welcome_page';
    private $_transient_id_update_welcome_page   = 'iio_update_welcome_page';

    /**
     *  Plugin configuration data.
     *  This is really should be constant, but to declare constant arrays PHP 5.6 is required.
     */
    private $_configuration = array(
        'id'                => 'innovationx-image-optimizer',
        'name'              => 'InnovationX Image Optimizer',
        'short_name'        => 'Image Optimizer',
        'version'           => '0.0.1',
        'db_version'        => 1,
        'option_name'       => 'innovationx_image_optimizer',
        'image_meta_key'    => 'innovationx_image_optimizer_metadata',
        'min_wp_version'    => '4.5',   // 4.5 - wp_add_inline_script(), 4.4 - wp_image_add_srcset_and_sizes()
        'min_php_version'   => '5.3',   // because of namespaces
        'pricing_plans_url' => 'https://imageoptimizerx.com/pricing'
    );

    public function GetConfiguration() { return $this->_configuration; }

    public function RequirementsMet()
    {
        if (version_compare(get_bloginfo('version'), $this->_configuration['min_wp_version'], '<') ||
            version_compare(PHP_VERSION, $this->_configuration['min_php_version'], '<')
        )
        {
            add_action('admin_init', array($this, 'SelfDeactivate'));
            add_action('admin_notices', array($this, 'ShowMinimumRequirementsNotice'));

            return false;
        }

        return true;
    }

    function SelfDeactivate()
    {
        deactivate_plugins(plugin_dir_path(dirname(__FILE__)) . 'innovationx-image-optimizer.php');
    }

    /**
     * Display an error message when the plugin deactivates itself.
     */
    function ShowMinimumRequirementsNotice()
    {
        require_once(plugin_dir_path(dirname(__FILE__)) . 'views/Startup/ShowMinimumRequirementsNoticeView.php');
    }

    public function Configure()
    {
        // Fires on plugin activation.
        register_activation_hook(plugin_dir_path(dirname(__FILE__)) . 'innovationx-image-optimizer.php', array($this, 'Activate'));

        // Fires on every plugin load.
        add_action('plugins_loaded', array($this, 'Update'));

        // Redirect to the Welcome page after plugin activation
        $this->ActivateWelcomePage();
    }

    function Activate()
    {
        $this->EnsureCreateOptions();

        // Activate the auto redirection to the welcome page
        set_transient($this->_transient_id_activate_welcome_page, true, 30);
    }

    /**
     * Initialize default option values
     *
     * @since      1.0.0
     */
    function EnsureCreateOptions()
    {
        $initialOptions = array(
            'version'                             => $this->_configuration['version'],
            'db_version'                          => $this->_configuration['db_version'],
            'api_key'                             => '',
            'is_automatically_optimize_on_upload' => true,
            'is_lossless'                         => false,
            'is_optimize_original'                => false,
            'is_optimize_original_only_lossless'  => false,
            'is_convert'                          => true,
            'is_convert_to_webp'                  => true,
            'is_convert_to_jxr'                   => true,
            'is_convert_to_apng'                  => true,
            'is_browser_compatibility_enabled'    => false,
            'permissions'                         => null,
            'is_debug'                            => false
        );

        add_option($this->_configuration['option_name'], $initialOptions);
    }

    function Update()
    {
        $this->_options = get_option($this->_configuration['option_name']);
        $this->RunUpdate();
    }

    /**
     * Run the incremental updates one by one.
     *
     * For example, if the current DB version is 1, and the target DB version is 3,
     * this function will execute update routines if they exist:
     *  - update_routine_2()
     *  - update_routine_3()
     */
    public function RunUpdate()
    {
        // Update plugin version
        $currentVersion = $this->_options['version'];

        if (version_compare($currentVersion, $this->_configuration['version'], '<'))
        {
            $this->_options['version'] = $this->_configuration['version'];
            update_option($this->_configuration['option_name'], $this->_options);

            // Activate the auto redirection to the welcome page
            set_transient($this->_transient_id_update_welcome_page, true, 30);
        }

        // Update plugin database
        $currentDbVersion = $this->_options['db_version'];
        if ($currentDbVersion < $this->_configuration['db_version'])
        {
            // No PHP timeout for running updates
            set_time_limit(0);

            // Run update routines one by one until the current version number reaches the target version number
            while ($currentDbVersion < $this->_configuration['db_version'])
            {
                $currentDbVersion++;

                // Each db version will require a separate update function for example,
                // for db_version 3, the function name should be UpdateRoutine3
                $updateFunctionName = 'UpdateRoutine' . $currentDbVersion;
                if (is_callable(array($this, $updateFunctionName)))
                {
                    call_user_func(array($this, $updateFunctionName));

                    // Update the options in the database, so that this process can always pick up where it left off
                    $this->_options['db_version'] = $currentDbVersion;
                    update_option($this->_configuration['option_name'], $this->_options);
                }
            }
        }
    }

    /*
     * Update routine for upcomming database version
     */
    function UpdateRoutine2()
    {

    }

    function ActivateWelcomePage()
    {
        add_action('admin_init', array($this, 'RedirectToWelcomePage'));
        add_action('admin_menu', array($this, 'AddWelcomePageMenu'));
        add_action('admin_head', array($this, 'RemoveWelcomePageMenu'));
    }

    function RedirectToWelcomePage()
    {
        // Bail if no activation redirect
        if (!get_transient($this->_transient_id_activate_welcome_page) &&
            !get_transient($this->_transient_id_update_welcome_page)
        )
            return;

        // Delete the redirect transient
        delete_transient($this->_transient_id_activate_welcome_page);
        delete_transient($this->_transient_id_update_welcome_page);

        // Bail if activating from network, or bulk
        if (is_network_admin() || isset($_GET['activate-multi']))
            return;

        // Redirect to the welcome page
        wp_safe_redirect(add_query_arg(array('page' => 'innovationx-image-optimizer-welcome'), admin_url('index.php')));
    }

    function AddWelcomePageMenu()
    {
        add_dashboard_page('InnovationX Image Optimizer: Welcome', 'InnovationX Image Optimizer: Welcome', 'read', 'innovationx-image-optimizer-welcome', array($this, 'AddWelcomePage'));
    }

    function AddWelcomePage()
    {
        require_once(plugin_dir_path(dirname(__FILE__)) . 'views/Startup/WelcomePageView.php');
    }

    function RemoveWelcomePageMenu()
    {
        remove_submenu_page('index.php', 'innovationx-image-optimizer-welcome');
    }
}
