<?php
// If uninstall is not called from WordPress, exit
if (!defined('WP_UNINSTALL_PLUGIN'))
    exit();

$optionName = 'innovationx_image_optimizer';
delete_option($optionName);

$imageMetaKey = 'innovationx_image_optimizer_metadata';
delete_post_meta_by_key($imageMetaKey);

delete_transient('iio_activate_welcome_page');
delete_transient('iio_update_welcome_page');