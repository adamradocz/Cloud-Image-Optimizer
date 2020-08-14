<?php
/*
Plugin Name: InnovationX Image Optimizer
Plugin URI:  http://wordpress.org/extend/plugins/innovationx-image-optimizer/
Description: Optimize your images, improve your website performance, boost your SEO and save money on your bandwidth. Simple, yet powerful.
Version:     0.0.1
Author:      InnovationX
Author URI:  https://imageoptimizerx.com/
Domain Path: /languages
Text Domain: innovationx-image-optimizer
License:     GPLv3
License URI: https://www.gnu.org/licenses/gpl-3.0.html


 InnovationX Image Optimizer Plugin
 Copyright (C) 2016, InnovationX
 
 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

// If this file is called directly, abort.
if (!defined('ABSPATH'))
	exit();

require_once('PSR-4-Autoloader.php');

$iioStartup = new InnovationX\ImageOptimizer\Startup();
if($iioStartup->RequirementsMet())
{
    $iioStartup->Configure();
    $iioMain = new InnovationX\ImageOptimizer\Main($iioStartup->GetConfiguration());
}