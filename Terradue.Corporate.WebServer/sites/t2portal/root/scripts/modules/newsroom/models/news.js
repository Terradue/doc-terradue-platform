
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		findAll: 'GET /'+Config.api+'/news/search',
		findOne: 'GET /'+Config.api+'/news/{id}',
		create: 'POST /'+Config.api+'/news',
		update: 'PUT /'+Config.api+'/news/{id}',
		destroy: 'DELETE /'+Config.api+'/news/{id}',
		
		search: function(q, page, count){
			return $.getJSON('/'+Config.api+'/news/search?q='+(q?q:'') + '&startPage='+(page?page:'') + '&count=' +(count?count:''));
		},
		
		searchOne: function(id){
			return $.getJSON('/'+Config.api+'/news/search?id='+id);
		},
		
		getTags: function(){
			return $.getJSON('/'+Config.api+'/news/tags');
		}
		
	}, {});
	
});

