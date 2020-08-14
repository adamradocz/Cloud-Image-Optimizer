<div class="error">
	<h3><?php echo esc_html($this->_configuration['name']); ?></h3>
    <p>The plugin has deactivated itself because your environment doesn't meet all of the requirements listed below.</p>
	<ul class="ul-disc">
		
        <li>
			<b >WordPress <?php echo $this->_configuration['min_wp_version']; ?>+</b> - You're running version <?php echo esc_html(get_bloginfo( 'version')); ?>
            <p><em>If you need help upgrading WordPress you can refer to <a href="http://codex.wordpress.org/Upgrading_WordPress">the Codex</a>.</em></p>
		</li>
        
        <li>
			<b >PHP <?php echo $this->_configuration['min_php_version']; ?>+</b> - You're running version <?php echo esc_html(PHP_VERSION); ?>
            <p><em>If you need to upgrade your version of PHP you can ask your hosting company for assistance.</em></p>
		</li>

	</ul>
</div>