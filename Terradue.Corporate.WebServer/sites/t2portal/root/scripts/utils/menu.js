
// Menu Object
define([
	'jquery',
	'underscore',
	'config',
	'underscorestring',
	//'bootstrapHoverDropdown'
], function($, _, Config){
	
	// merge string plugin to underscore namespace
	_.mixin(_.str.exports());

	return {
		
		activate: function(selector) {
			var self = this;
			
			// load menu
			if (!this.menu && this.menu!='loading'){
				this.menu = 'loading';
				$.get(Config.menuUrl, function(json){
					self.showMenu(json);
					self.activate(selector);
				}, 'json');
				return;
			}
			
			_.mixin(_.str.exports());
			
			var activated = false;
			
			//ENSURE NAV PARAM IS A JQUERY OBJECT
			var $el = selector instanceof $ ? selector : $(selector);
			
			//ACTIVATE NAV BUTTON
			$el.each(function() {
				//GET LINK AND COMPARE AGAINST URL
				var url = _.ltrim($('a', this).attr('href'), '#!');
				var hash = _.ltrim(window.location.hash, '#!');
				
				//MATCHES ROUTES TO NAV LINKS
				if (hash === url) {
					//REMOVE ACTIVE CLASS FROM SIBLINGS
					$(this).addClass('active').siblings().removeClass('active');
					activated = true;
					return false;
				}
			});
			
			//REMOVE ACTIVE INDICATOR SINCE NOT ON A PAGE FROM THE NAV
			if (!activated) $el.removeClass('active');
		},
		
		showMenu: function(menu){
			this.menu = menu;
			console.log(menu);
			window.menu=menu;
			/*
			 * 
			 * 
                      <li class="active"><a href="#">Home</a></li>
                      <li><a href="#">Link</a></li>
                      <li><a href="#">Link</a></li>
                      <li class="dropdown">
                        <a href="#" class="dropdown-toggle" data-toggle="dropdown">Dropdown <b class="caret"></b></a>
                        <ul class="dropdown-menu">
                          <li><a href="#">Action</a></li>
                          <li><a href="#">Another action</a></li>
                          <li><a href="#">Something else here</a></li>
                          <li class="divider"></li>
                          <li class="nav-header">Nav header</li>
                          <li><a href="#">Separated link</a></li>
                          <li><a href="#">One more separated link</a></li>
                        </ul>
                      </li>

			 */
			
			var getUrl = function(link){
				return (link==null ? 'javascript://' : '#!'+link);
			}
			
			// first menu
			$.each(menu, function(){
				var $li = $('<li>').appendTo($("#menu>.nav")),
					link = this.Link;
				
				if (this.Subtitles){
					// add dropdown
					$li.addClass("dropdown");
					var $dropDown = $('<a href="'+getUrl(link)+'" class="dropdown-toggle" data-target="#" data-toggle="dropdown">'
							+ this.Title +' <b class="caret"></b></a>')
							.appendTo($li);
							
					var $ul = $('<ul class="dropdown-menu">').appendTo($li);
					// iterate subtitles
					var thereIsDivider = false;
					$.each(this.Subtitles, function(){
						// iterate sub-subtitles
						if (this.Subtitles){
							$ul.append('<li class="divider"></li>');
							$ul.append('<li class="nav-header">'+this.Title+'</li>');
							$.each(this.Subtitles, function(){
								$ul.append('<li class="indent"><a href="'+getUrl(this.Link)+'">'+this.Title+'</a></li>');
							});
						} else
							$ul.append('<li><a href="'+getUrl(this.Link)+'">'+this.Title+'</a></li>');
					});
					$dropDown.dropdownHover();
					if (link)
						$dropDown.click(function(){
							window.location.hash = getUrl(link);
						});
					
				} else
					$li.append('<a href="'+getUrl(link)+'">'+this.Title+'</a>');
			});
			
			
			// second menu
			$.each(menu, function(){
				var $li = $('<li>').appendTo($("#menuPhone>.nav")),
					link = this.Link;
				
				if (this.Subtitles){
					// add dropdown
					$li.addClass("dropdown");
					var $dropDown = $('<a href="'+getUrl(link)+'" class="dropdown-toggle" data-target="#" data-toggle="dropdown">'
							+ this.Title +' <b class="caret"></b></a>')
							.appendTo($li);
							
					var $ul = $('<ul class="dropdown-menu">').appendTo($li);
					// iterate subtitles
					var thereIsDivider = false;
					$.each(this.Subtitles, function(){
						// iterate sub-subtitles
						if (this.Subtitles){
							$ul.append('<li class="divider"></li>');
							$ul.append('<li class="nav-header">'+this.Title+'</li>');
							$.each(this.Subtitles, function(){
								$ul.append('<li class="indent"><a href="'+getUrl(this.Link)+'">'+this.Title+'</a></li>');
							});
						} else
							$ul.append('<li><a href="'+getUrl(this.Link)+'">'+this.Title+'</a></li>');
					});
					if (link)
						$dropDown.click(function(){
							window.location.hash = getUrl(link);
						});
					
				} else
					$li.append('<a href="'+getUrl(link)+'">'+this.Title+'</a>');
			});
		}	
	
	}
});
