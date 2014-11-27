define([
	'jquery',
	'can',
	'underscore',
	'geobrowser/config',
	'utils/helpers',
	'modules/login/models/login',
	'geobrowser/wpsControl',
	'geobrowser/featuresBasketControl',
	'geobrowser/densityMapLayer',
	'jqueryUI',
	'leaflet',
	'opensearchNavigator',
	'wpsSlurper',
	'jqueryFullscreen',
	'jqueryLayout',
	'jqrange',
	'leafletMarkerCluster',
	'leafletDvf',
	'jsonViewer',
	'leafletGoogle',
], function(
	$,
	can, 
	_,
	Config,
	Helpers,
	LoginModel,
	WpsControl,
	FeaturesBasketControl,
	DensityMapLayer
){

return {
	
	init: function(){
		
		var self = this,
			$div = $(Config.selector);
		
		window.Helpers = Helpers; // TODO toremove
		Helpers.logoInConsole();
		
		// set the identifier name
		can.Model.id = 'Id';
		
		// configure ajax requests
		$.ajaxSetup({
		    contentType : 'application/json',
		    processData : false,
		});
		$.ajaxPrefilter( function( options, originalOptions, jqXHR ) {
		    if (options.data)
		        options.data=JSON.stringify(options.data);
		});
		
		// set canjs view extension to nothing for extensionless view filenames
		can.view.ext = '';
		// set default template engine for can.view
		can.view.types[''] = can.view.types['.' + Config.template];
		
		// opensearch url
		LoginModel.isLogged(function(){

			self.initLayout();
			
			self.initFeaturesBasket();
			
			self.initMap();
			
		}).fail(function(){
			$div.append('<div class="alert alert-block alert-error"><h4>Warning!</h4><br/>'
					+'<strong>You are not logged.</strong><br/>Please login to the <a href="/">home page</a>.</div>');
		});
		
	},
	
	initLayout: function(){
		var self = this,
			$divLayout = $(Config.layoutSelector),
			outerLayout = $divLayout.layout({
				north: {
					size: 47,
					resizable: false,
					spacing_open: 3,
					spacing_closed: 3,
				},
				east: {
					size: 450,
					initClosed:true,
					spacing_closed: 24,
					togglerLength_closed: 210,
					togglerContent_closed: '<div class="wpsServiceLabel">Processing Services</div>',
					togglerAlign_closed: 'middle',
					onopen: function(){
						if (!self.wpsControl)
							self.initWpsControl();
						self.wpsControl.triggerTabShow();
						return true;
					},
					onclose: function(){
						if (self.wpsControl)
							self.wpsControl.triggerTabHide();
					}
				},
				center: {
					onresize: function(){
						self.resize();
					},
				}
			}),
			innerLayout = $divLayout.children('.ui-layout-center').layout({
				south: {
					size: 200,
					initClosed: true,
				},
				center: {
					onresize: function(){
						self.resize();
					}
				}
			});			
	
		// show the sout content
		$('.ui-layout-south>div').show();
		
		this.outerLayout = outerLayout;
		this.innerLayout = innerLayout;
	},
	
	initMap: function(){
    	var self = this,
    		firstTimeOpenFeature = true,
    		$div = $(Config.selector),
    		pointToLayer = function (feature, latlng) {
	    		return L.circleMarker(latlng, {
	    		    radius: Math.exp(feature.properties.mag/2.3),
	    		    fillColor: "#cc00cc",
	    		    color: "#000",
	    		    weight: 1,
	    		    opacity: 1,
	    		    fillOpacity: 0.8
	    		});
	        	//return L.marker(latlng,  {icon: self.volcanoIcon});
	    	},
	    	popupContent = function(id, properties){
				var ref = Helpers.getHrefFromLinks(properties.links, 'self'),
					$div = $('<div><i class="osn-popup-icon icon-info-sign icon-2x"></i>'
							+ '<h1 class="osn-popup-title textOverflow">Feature ' + properties.title + '</h1>'
							+ '<span class="links"><a href="#" class="switchView jsonViewerView">Switch view</a> | '
							+		'<a href="'+ref+'" target="_blank" class="openJsonFile">Open JSON file</a></span>'
							+ '</div>'),
					$jsonDiv = $('<div>').addClass('jsonDiv').appendTo($div);
				
				$div.find('.switchView').click(function(){
					if ($(this).hasClass('jsonViewerView')){
						$jsonDiv.html(L.HTMLUtils.buildTable(properties));
						$(this).removeClass('jsonViewerView');
					} else {
						$jsonDiv.empty().jsonViewer(properties);
						$(this).addClass('jsonViewerView');
					}
				});
				$jsonDiv.jsonViewer(properties);
				
			
				return $div.get(0);
			};
			
		this.$osn = $("<div>").appendTo($div).opensearchNavigator({
			osDescription: Config.opensearchUrl,
			baseMaps: {
				'Example Mapbox': 'mapbox:examples.map-i875mjb7',
				'Natural Earth': 'http://otile4.mqcdn.com/tiles/1.0.0/sat/{z}/{x}/{y}.jpg',
				'OpenStreetMap': 'http://{s}.tile.osm.org/{z}/{x}/{y}.png?',
								  //http://otile1.mqcdn.com/tiles/1.0.0/osm/{z}/{x}/{y}.png',
				'Google Road Map': 'google:ROADMAP',
				'Custom Mapbox': 'ciccioceras.iea6a6e1', 
			},
			resultsSelectable: {
				onFeaturesSelected: function(features){
					if (firstTimeOpenFeature){
						self.innerLayout.open("south");
						firstTimeOpenFeature = false;
					}
				},
			},
			resultsDraggable: {
				onFeaturesDragStart: function(features){
					self.draggedFeatures = features;
				},
				onFeaturesDragStop: function(features){
				},
				revert: 'invalid',
			},
			onMapLoad: function(map, data){
				self.map = map;
				self.addFullscreenButton(map);
				self.addDateSlider(map);
				self.addThematic(map);
				self.$osfi = data.$osfi; // opensearch form it! object
				self.layerControl = data.layerControl;
				self.addOptionalLayers(map);
				
				self.initRouting();
				
			},
			searchOnMapMove: true,
			panOnFeatures: false,
			zoomPosition: 'topleft',
			drawPosition: 'topleft',
			
			resultsJsonTableOptions: {
				columnsToShow: ['title'],
				tableClass: '',
				showHeader: false,
//				rowRenderer: function($tr, jsonRow){
//					$tr.attr({
//						'data-id': jsonRow.id,
//						'data-ref': Helpers.getHrefFromLinks(jsonRow.links, 'self'),
//					});
//				},
			},
			tableOffset: -25,
			pointToLayer: pointToLayer,
			pagination: true,
			highlightedFeatureStyle: { color: 'white', fillOpacity: 0.5, weight: 4, dashArray: '' },
			hoveredFeatureStyle: { color: "red", fillOpacity: 0.6, weight:3 },
			searchDoneCallback: function(json, generatedUrl){
				self.innerLayout.open("south");
				
				if (Config.densityMapEnabled){
					if ( json.properties.totalResults > 20 )
						DensityMapLayer.update(generatedUrl.split('?')[1]);
					else
						DensityMapLayer.removeData();
				}
			},
			
			externalResultsControl: "#osResults",
			
			popupContent: popupContent,
		});
	},
	
	resize: function(){
		if (this.map)
			this.map.invalidateSize();
		if (this.$rangeSlider)
			this.$rangeSlider.dateRangeSlider('resize');
	},
	
	volcanoIcon: L.icon({
	    iconUrl: 'styles/img/triangleIcon2.png',
	    //shadowUrl: 'styles/img/triangleIconShadow.png',

	    iconSize:     [30, 30], // size of the icon
	    shadowSize:   [30, 30], // size of the shadow
	    iconAnchor:   [25, 25], // point of the icon which will correspond to marker's location
	    shadowAnchor: [25, 25],  // the same for the shadow
	    popupAnchor:  [-3, -76] // point from which the popup should open relative to the iconAnchor
	}),
	
	addFullscreenButton: function(map){
		var self = this;
		// add the fullscreen feature
		new L.Control.Button({
			position: "topleft",
			icon: "icon-fullscreen",
			handler: function(){
				if ($.fullscreen.isFullScreen())
					$.fullscreen.exit();
				else
					$(Config.layoutSelector).fullscreen();
				self.resize();
			},
		}).addTo(map);
	},
	
	addThematic: function(map){
		var $osn = this.$osn,
			$div = $('<div class="thematic">').appendTo($('#topBar')),
			$ul = $('<ul>').appendTo($div);
		
		$.each(Config.contexts, function(){
			var search = this.search,
				$li = $('<li><a href="#context='+this.id+'">' + this.name + '</a></li>')
				.appendTo($ul);
//				.click(function(){
//					App.$osfi.setParameterValues(search, true);
//				});
		});
	},
	
	addDateSlider: function(map){
		var $osn = this.$osn,
			$div = $('<div class="jqrangeContainer">').appendTo($osn),
			now = new Date(),
			bounds = {
				min: new Date(1980, 0, 1),
				max: new Date(now.getFullYear(), now.getMonth(), now.getDate()),
			};
		
		$div.dateRangeSlider({
			bounds: bounds,
			defaultValues: bounds,
			step:{
				days: 1,
			},
		}).bind("valuesChanged", function(e, data){
			var start = data.values.min,
				end = data.values.max;
			
			if (+start === +bounds.min)
				App.$osfi.removeParameterValue('time:start');
			else
				App.$osfi.setParameterValue('time:start', start.toISOString());

			if (+end === +bounds.max)
				App.$osfi.removeParameterValue('time:end', true);
			else
				App.$osfi.setParameterValue('time:end', end.toISOString(), true);
		});
		
		this.$rangeSlider = $div;
	},
	
	initRouting: function(){
		can.route.bind('change', function(ev, attr, how, newVal, oldVal) {
			var routeAttr = can.route.attr();
			console.log('CHANGE!', attr, how, newVal, oldVal);
			
			// context changing
			if (attr=='context'){
				var contextId = newVal,
					contextsFound = $.grep(Config.contexts, function(c){return (c.id == contextId)});
				if (contextsFound.length)
					App.$osfi.setParameterValues(contextsFound[0].search, true);
			}
		});

		can.route.ready();
	},
	
	initFeaturesBasket: function(){
		var self = this;
		
		this.featuresBasketControl = new FeaturesBasketControl($("#featureContainer"), {
			openWps: function(){
				self.outerLayout.open('east');
				self.outerLayout.sizePane('east', 450);
			},
		});

		// droppable effect on basket
		$('#featureContainer').droppable({
			accept: "#osResults tr",
			activeClass: "dropActive",
			hoverClass: "dropHover",
			drop: function(event, ui) {
				if (self.draggedFeatures)
					self.featuresBasketControl.addFeatures(self.draggedFeatures);
			}
		});
		
		$('#featureContainer .featuresBasketTable').draggable({
			cursor: 'move',
			cursorAt: { top: 9, left: 12 },
			helper: function(a,b,c){
				var features = self.featuresBasketControl.getFeatures();
				return $('<div class="osn-dragHelper">')
					.html('<i class="fa fa-arrows"></i> Basket of ' 
						+ features.length
						+ ' feature' + (features.length==1 ? '' : 's'));
			},
			appendTo: 'body',
		});
	},
	
	initWpsControl: function(){
		var self = this;
		this.wpsControl = new WpsControl($("#servicesPanel"), {
			getFeaturesFromSelection: function(){
				return self.draggedFeatures;
			},
			getFeaturesFromBasket: function(){
				return self.featuresBasketControl.getFeatures();
			}
		});
	},

	addOptionalLayers: function (map){
		// add results layer inside control layer
//		alert(this.$osn.data.resultsLayer);
		
		this.disasterCharterActivationsLayer(this.layerControl, map);
		this.significantEarthquakesLayer(this.layerControl);
		
		if (Config.densityMapEnabled)
			DensityMapLayer.init(this.layerControl, map);
	},

	disasterCharterActivationsLayer: function(controlLayer, map){
		var markers = new L.MarkerClusterGroup();
		var self = this;

		$.getJSON( "/t2api/disastercharter", function( data ) {

			var geojson = L.geoJson(data, {

				style: function (feature) {
					return {color: feature.properties.color};
				},

				onEachFeature: function (feature, layer) {

					var popupText = '<a href="' + feature.properties.link + '" target="_blank">' + feature.properties.title + '</a><br>';
					var $popupDiv = $("<div>").append(popupText);
					$("<button class='btn btn-mini btn-info'><i class='icon-search'></i> Search data for event</button>").appendTo($popupDiv).click(function(){
					// update range slider
						var start = moment(new Date(feature.properties.disasterdate)).subtract(1, 'month'), // 1 month before
							end = moment(new Date(feature.properties.disasterdate)).add(1, 'month'); // 1 month later
						self.$rangeSlider.dateRangeSlider('values', start.toDate(), end.toDate());
						var geometry = "POINT("+feature.geometry.coordinates[0]+" "+feature.geometry.coordinates[1]+")";
						self.$osfi.setParameterValue('geo:geometry', geometry, true);
					});
					
					layer.bindPopup($popupDiv.get(0));
				}
			});
			markers.addLayer(geojson);
			map.addLayer(markers);
			controlLayer.addOverlay(markers, "Charter Activations");
		});


	},

	significantEarthquakesLayer: function(controlLayer){

		var markers = new L.MarkerClusterGroup();
		var self = this;

		$.getJSON( "/t2api/earthquakes", function( data ) {

			var geojson = L.geoJson(data, {

				style: function (feature) {
					return {
						"color": "#ff7800",
    					"weight": 5,
    					"opacity": 0.65};
				},

				onEachFeature: function (feature, layer) {

					
					var popupText = '<a href="' + feature.properties.url + '" target="_blank">' + feature.properties.title + '</a><br>';
					var $popupDiv = $("<div>").append(popupText);
					$("<button class='btn btn-mini btn-info'><i class='icon-search'></i> Search data for event</button>").appendTo($popupDiv).click(function(){
					// update range slider
						var start = moment(new Date(feature.properties.time*1)).subtract(1, 'month'), // 1 month before
							end = moment(new Date(feature.properties.time*1)).add(1, 'month'); // 1 month later
						self.$rangeSlider.dateRangeSlider('values', start.toDate(), end.toDate());
						var geometry = "POINT("+feature.geometry.coordinates[0]+" "+feature.geometry.coordinates[1]+")";
						self.$osfi.setParameterValue('geo:geometry', geometry, true);
					});
					
					layer.bindPopup($popupDiv.get(0));


				},

				pointToLayer: function(feature, latlng) {
			        return L.circleMarker(latlng, {
			            // Here we use the `count` property in GeoJSON verbatim: if it's
			            // to small or two large, we can use basic math in Javascript to
			            // adjust it so that it fits the map better.
			            radius: feature.properties.mag * 10
			        })
			    }
			});
			markers.addLayer(geojson);

		});

		controlLayer.addOverlay(markers, "Significant Earthquakes");
	}
};
});
