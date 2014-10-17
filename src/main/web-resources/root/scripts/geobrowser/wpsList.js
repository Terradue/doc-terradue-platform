define([
	'jquery',
	'can',
	'geobrowser/config',
	'utils/helpers',
	'wpsSlurper',
],function($, can, Config, Helpers){
	
	var WpsListControl = can.Control({
		
		init: function(element, options){
			console.log("wpsList.init");
			this.services = new can.Observe.List([]);
			this.loaded = false;
			this.initWps();
		},
		
		icons: [
			'area-chart',
			'bus',
			'google-wallet',
			'share-square',
			'bomb',
			'ils',
			'calculator',
			'pie-chart',
			'bookmark',
			'cube',
			'dashboard',
			'send',
			'ra',
			'renren',
		],
		colors: [
			'#F60',
			'#FFAD00',
			'#00D6FF',
			'#0085FF',
			'#A300FF',
			'#FF007A',
			'#215DAA',
			'#3CAA21',
			'#7197B1',
			'#015A0B',
		],
		
		initWps: function(){
			var self = this, options = this.options;
			
			if (!this.loaded){
				this.element.mask('loading...');
				
				// load the list of wps services
				$.get('/t2api/service/wps/search?format=json', function(response){
					$.each(response.features, function(){
						self.services.push({
							wpsIdentifier: this.properties.identifier,
							title : this.properties.title,
							url: this.properties.links[0]['@href'],
						});
					});
					self.services.push({
						wpsIdentifier: 'fakeWps',
						url: '/scripts/geobrowser/wpsCache1',
						isFake: true,
					});

					self.loaded = true;
					self.initWps();
//					$op.unmask().append(can.view('/scripts/geobrowser/views/selectedFeatures.html', operations))
				});
				return;
			}
			
			// for each wps, load info (processes list)
			for (i=0; i<this.services.length; i++){
				var service = this.services[i];
				if (!service.wps){
					//service.url = 'http://gpod.eo.esa.int/wps/?service=WPS&request=GetCapabilities';
						//'http://127.0.0.1:8888/WpsHadoop_trunk/wps/WebProcessingService?Request=GetCapabilities&service=WPS&version=1.0.0';//
					WpsSlurper.parseCapabilities(service.url, function(wps){
						$.each(wps.processes, function(){							
							this.wps = wps;
							var hashCode = Math.abs(this.identifier.hashCode());
							this.icon = self.icons[hashCode % self.icons.length];
									//Helpers.random(self.icons);
							this.iconBackground = self.colors[hashCode % self.colors.length];
									//Helpers.random(self.colors);
						});
						wps.service = service;
						service.attr('wps',wps);
						self.initWps();
					}, function(){
						service.attr('wps', {}); // do nothing for now, TODO improve
						self.initWps();						
					}, true, service.isFake);
					return;
				}
			}
			
			// show view
			window.services = this.services;
			this.element.unmask();
			this.element.html(can.view('/scripts/geobrowser/views/services.html', this.services));
		},

		'.serviceItem click': function(el){
			if (this.options.processSelected)
				this.options.processSelected(el.data('wps-id'), el.data('process-id'));
		},
		
	});
	
	return WpsListControl;
	
});
