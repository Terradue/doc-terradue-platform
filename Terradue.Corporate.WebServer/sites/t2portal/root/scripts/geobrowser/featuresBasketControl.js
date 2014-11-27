define([
	'jquery',
	'can',
	'geobrowser/config',
	'geobrowser/models/dataPackage',
	'utils/helpers',
	'geobrowser/wpsList',
	'bootstrap',
	'multiFieldExtender',
	'bootbox',
],function($, can, Config, DataPackageModel, Helpers, WpsListControl){
	
	
	
var FeaturesBasketControl = can.Control({
	
	init: function(element, options){
		var self = this;
		
		this.featuresInBasket = new can.Observe.List([]);
		element.html(can.view('/scripts/geobrowser/views/featuresBasket.html', this.featuresInBasket));
		
	},
	
	addFeatures: function(features){
		var self = this;
		$.each(features, function(){
			var feature = this;
			// push only if not duplicate
			if ($.grep(self.featuresInBasket, function(f){return (f.id==feature.id)}).length==0)
				self.featuresInBasket.push(feature);		
		});
	},
	
	getFeatures: function(){
		return this.featuresInBasket;
	},
	
	'.showServices click': function(){
		this.options.openWps();
	},
	
	'.removeItem click': function(element){
		var id = element.parent().data('id'), self = this;
		this.featuresInBasket.each(function(f, i){
			if (f.id==id){
				self.featuresInBasket.splice(i, 1);
				return false;
			}
		});
	},
	
	'.removeAll click': function(){
		while(this.featuresInBasket.pop());
	},
	
	'.loadDataPackage click': function(){
		var self = this;
		DataPackageModel.findAll({}, function(dataPackages){
			$('#dataPackageSelectorModal').modal();
			$('#dataPackageSelectorModal .dataPackages')
				.html(can.view('/scripts/geobrowser/views/dataPackages.html', dataPackages));			
			
			$('#dataPackageSelectorModal .dataPackagesList a').click(function(){
				var id=$(this).data('id');
				
				try{
					var dp = $.grep(dataPackages, function(dp){ return dp.Id == id; })[0];
					
					// to mantain compatibility with feature data
					dp.Items.each(function(item, i){
						item.attr({
							id: item.Location,
							properties: {
								title: item.Name ? item.Name : (item.Identifier ? item.Identifier : item.Location)
							},
						});
					});
					self.addFeatures(dp.Items);
					
					$('#dataPackageSelectorModal').modal('hide');
					
				} catch(e){};
			});
			
//			self.element.find('.dataPackages').fadeIn()
//			.html(can.view('/scripts/geobrowser/views/dataPackages.html', dataPackages));			

		});
	},
	
	saveDataPackage: function(){
		
	},
	
	'.saveDataPackage click': function(){
		var self = this,
			features = this.featuresInBasket
			$modal = $('#dataPackageFormModal'),
			$errorText = $('#dataPackageFormModal form .text-error');
		if (features.length==0)
			return;
		
		$modal.modal();
		$errorText.empty();
		$('#dataPackageFormModal form button[type="submit"]').unbind('click').click(function(){
			var data = Helpers.retrieveDataFromForm($('#dataPackageFormModal form'), ['Name', 'IsDefault']);
			if (data.Name){
				
				data.Identifier = data.Name;
				
				// creating data package object
				data.Items = [];
				features.each(function(f){
					data.Items.push({
						Location: f.id,
						Identifier: f.properties.identifier,
						Name: f.properties.title,
					});
				});
				
				// saving data into data package
				$modal.mask('Saving...');
				new DataPackageModel(data).save().then(function(){
					$modal.unmask();
					$modal.modal('hide');
					bootbox.alert('Data Package created.');
				}).fail(function(xhr){
					$modal.unmask();
					$errorText.text(Helpers.getErrMsg(xhr, 'Error to save DataPackage.'));
				});
				
				window.features=features; // TODO: toremove
				window.data=data;
				
			} else
				$errorText.text('Identifier and Name are mandatory.');
			return false;
		});
		window.features = features;
	},
	
});
	
return FeaturesBasketControl;
	


});
