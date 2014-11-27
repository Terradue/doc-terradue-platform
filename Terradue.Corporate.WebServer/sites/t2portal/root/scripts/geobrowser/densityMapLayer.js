
define([
	'jquery',
	'can',
	'geobrowser/config',
	'opensearchSlurper',
	'leafletHeat',
],function($, can, Config){
	
return {
		
	init: function(controlLayer, map){
		
		this.densityOsUrl = "http://data.terradue.com/ec/eo/manu/density"

		var cfg = {
		  // radius should be small ONLY if scaleRadius is true (or small radius is intended)
		  // if scaleRadius is false it will be the constant radius used in pixels
		  "radius": 100,
		  "maxOpacity": .3, 
		  // scales the radius based on map zoom
		  "scaleRadius": false, 
		  // if set to false the heatmap uses the global maximum for colorization
		  // if activated: uses the data maximum within the current map boundaries 
		  //   (there will always be a red spot with useLocalExtremas true)
		  "useLocalExtrema": false,
		  // which field name in your data represents the latitude - default "lat"
		  latField: 'lat',
		  // which field name in your data represents the longitude - default "lng"
		  lngField: 'lng',
		  // which field name in your data represents the data value - default "value"
		  valueField: 'count'
		};

		this.heatmapLayer = new HeatmapOverlay(cfg);
		map.addLayer(this.heatmapLayer);
		controlLayer.addOverlay(this.heatmapLayer, "Density Map");

	},
	
	update: function(queryString){
		var self=this;
		
		$.getJSON( this.densityOsUrl + "?div=25&" + queryString, function( data ) {
			self.heatmapLayer.setData(data);
		});
	},

	removeData: function(){
		this.heatmapLayer.setData({max:0, data: []});
	}
};
	
});