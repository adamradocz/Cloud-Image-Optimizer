<div class="wrap">
    <div id="icon-options-general" class="icon32"><br /></div>
    <h1><?php echo esc_html( $this->_name ); ?>: Settings</h1>
    <form method="post" action="options.php">
        <?php
        // This prints out all hidden setting fields
        settings_fields($this->_optionsGroup);
        do_settings_sections($this->_adminPageSlug);
        submit_button();
        ?>
    </form>
</div>

<div class="wrap">
	
	<div id="poststuff">	
		<div id="post-body" class="metabox-holder columns-2">		
			<!-- main content -->
			<div id="post-body-content">				
				<div class="meta-box-sortables ui-sortable">	

                    <div class="postbox">
						<!-- Toggle -->
                        <h2 class="hndle"><span>Cloud</span></h2>
						<div class="inside">
                            
						</div> <!-- .inside -->
					</div> <!-- .postbox -->
                    
				</div> <!-- .meta-box-sortables .ui-sortable -->				
			</div> <!-- post-body-content -->
			
			<!-- sidebar -->
			<div id="postbox-container-1" class="postbox-container">				
				<div class="meta-box-sortables">					
					<div class="postbox">
						<div class="handlediv" title="Click to toggle"><br></div>
						<!-- Toggle -->
						<h2 class="hndle"><span>Statistics</span></h2>
						<div class="inside">
						</div> <!-- .inside -->						
					</div> <!-- .postbox -->					
				</div> <!-- .meta-box-sortables -->				
			</div> <!-- #postbox-container-1 .postbox-container -->
			
		</div> <!-- #post-body .metabox-holder .columns-2 -->		
		<br class="clear">
	</div> <!-- #poststuff -->
	
</div> <!-- .wrap -->