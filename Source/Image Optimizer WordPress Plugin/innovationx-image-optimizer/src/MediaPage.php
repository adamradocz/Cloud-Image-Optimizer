<?php

namespace InnovationX\ImageOptimizer;
    
class MediaPage
{
    private $_id;
    private $_version;
    private $_name;
    private $_imageMetaKey;
    private $_pricingPlansUrl;
    private $_options;
    private $_optimizer;
    private $_user;

    public function __construct($configuration, $optimizer, $user)
    {
        $this->_id = $configuration['id'];
        $this->_version = $configuration['version'];
        $this->_name = $configuration['name'];
        $this->_imageMetaKey = $configuration['image_meta_key'];
        $this->_pricingPlansUrl = $configuration['pricing_plans_url'];
        $this->_options = get_option($configuration['option_name']);
        $this->_optimizer = $optimizer;
        $this->_user = $user;

        // Enqueue Media Library scripts
        add_action('admin_enqueue_scripts', array($this, 'EnqueueAdminMediaScripts'));

        //Handle Optimize Now button click (Ajax call)
		add_action('wp_ajax_OptimizeMediaLibraryImage', array($this, 'OptimizeMediaLibraryImage'));
        
        add_filter('manage_media_columns', array($this, 'AddColumnsToMediaLibrary'));
        add_action('manage_media_custom_column', array($this, 'ManageCustomColumnsInMediaLibrary'), 10, 2);
    }
    
    function EnqueueAdminMediaScripts($hook)
    {
        // Only applies to Upload panel
        if($hook !== 'upload.php')
	        return;

        $scriptName = $this->_options['is_debug'] === true ? 'admin-media.js' : 'admin-media.min.js';
        wp_enqueue_script($this->_id . 'admin-media', plugins_url('/js/' . $scriptName, dirname(__FILE__) ), array('jquery'), $this->_version);
        
        // in JavaScript, object properties are accessed as ajax_object.ajax_url, ajax_object.nonce
	    wp_localize_script($this->_id . 'admin-media', 'ajax_object', array( 
            'ajax_url' => admin_url('admin-ajax.php'),
            'nonce' => wp_create_nonce('OptimizeMediaLibraryImage'),
            'loaderImageHtml' => '<img src="' . plugins_url('/images/loading.gif', dirname(__FILE__) ) . '" >'
            )
        );
    }

    function OptimizeMediaLibraryImage()
    {
        // Verifies the AJAX request to prevent processing requests external of the blog.
        check_ajax_referer('OptimizeMediaLibraryImage', 'nonce');
         
        if(!current_user_can('upload_files'))
		    wp_die('You do not have sufficient permissions to work with uploaded files.');
        
        $attachmentId  = (int)$_POST['attachment_id'];
        $metadata      = wp_get_attachment_metadata($attachmentId);
        
        $this->_optimizer->OptimizeImage($metadata, $attachmentId);
        $html = $this->GenerateCustomColumnInfoInMediaLibrary($attachmentId);

        $responseData = array(
            'id'    =>  $attachmentId,
            'html'  =>  $html
        );
        
        // Send a JSON response back to an AJAX request, and die().
        wp_send_json($responseData);
    }
    
    /**
	* Print InnovationX Image Optimize column header in the media library
	* using the 'manage_media_columns' hook.
	*/
    function AddColumnsToMediaLibrary($columns)
    {
	    $columns[$this->_imageMetaKey] = $this->_name;	    
        return $columns;
    }

    /**
	* Print InnovationX Image Optimizer column data in the media library
	* using the 'manage_media_custom_column' hook.
	*/
	function ManageCustomColumnsInMediaLibrary($columnName, $attachmentId)
    {
		if($columnName === $this->_imageMetaKey)
		{
            echo $this->GenerateCustomColumnInfoInMediaLibrary($attachmentId);
        }
	}

    function GenerateCustomColumnInfoInMediaLibrary($attachmentId)
    {
        $iioMetadata = get_post_meta($attachmentId, $this->_imageMetaKey, true);
                    
        if(empty($iioMetadata))
        {
            $html = '';
            if ($this->_options['api_key'] === '')
            {
                $pluginSettingsUrl = esc_url(admin_url('options-general.php?page=' . $this->_id), array('http', 'https'));
                $html = '
                    <div>
                        <a href="' . $pluginSettingsUrl . '" >
                            <p class="button button-primary">Enter your API Key</p>
                        </a>
                    </div>
                ';
            }
            else if (!$this->_user->CanOptimizeOnCloud($attachmentId))
            {
                $html = '
                    <div>
                        <a href="' . esc_url($this->_pricingPlansUrl, array('http', 'https')) . '" target="_blank">
                            <p class="button button-primary iio-upgrade-now" id="' . esc_attr($attachmentId) . '">Upgrade Now</p>
                        </a>
                    </div>
                ';
            }
            else
            {
                $html = '
                    <div>
                        <p class="button button-primary iio-optimize-now" id="' . esc_attr($attachmentId) . '">Optimize Now</p>
                    </div>
                ';
            }

            return $html;
        }
        
        $totalSavings                    = $iioMetadata['total_savings'];
        $totalSavingsPercent             = $iioMetadata['total_savings_percent'];
        $totalSavingsPercentReadable     = round($totalSavingsPercent * 100, 2);
            
        $html = '
            <div>
                <p>Savings: ' . esc_html(size_format($totalSavings, 2)) . ' (' . esc_html($totalSavingsPercentReadable) . ' %) <br>
                Optimized images: ' . esc_html(count($iioMetadata['sizes'])) . ' </p>
            </div>
        ';

        return $html;
    }

}
