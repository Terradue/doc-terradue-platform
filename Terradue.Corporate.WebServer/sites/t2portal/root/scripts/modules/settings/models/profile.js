
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		update: 'PUT /'+Config.api+'/user',

		changeEmail: function(email){
			return $.ajax('/'+Config.api+'/user/email', {
				type : "PUT",
				dataType : "json",
				data : {Email:email}
			});
		},

		getApiKey: function(password){
			return $.ajax('/'+Config.api+'/user/apikey?password='+password, {
				type : "GET",
				dataType : "json",
				format : "json"
			});
		},

		generateApiKey: function(password){
			return $.ajax('/'+Config.api+'/user/apikey', {
				type : "PUT",
				dataType : "json",
				format : "json",
				data : {
					Password:password
				}
			});
		},

		revokeApiKey: function(password){
			return $.ajax('/'+Config.api+'/user/apikey?format=json&password='+password, {
				type : "DELETE",
				dataType : "json"
			});
		},
		
		getCatalogueIndex: function(){
			return $.getJSON('/'+Config.api+'/user/catalogue/index');
		},
		createCatalogueIndex: function(){
			return $.post('/'+Config.api+'/user/catalogue/index?format=json', {});
		},
		
		getRepository: function(){
			return $.getJSON('/'+Config.api+'/user/repository');
		},
		createRepository: function(){
			return $.post('/'+Config.api+'/user/repository?format=json', {});
		},
		
		getFeatures: function(){
			return $.getJSON('/'+Config.api+'/user/features');
		},
		createFeatures: function(){
			return $.post('/'+Config.api+'/user/features/geoserver?format=json', {});
		}
		
	}, {});
	
});
