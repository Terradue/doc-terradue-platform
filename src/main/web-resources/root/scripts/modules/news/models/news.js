
define(['can', 'configurations/news'], function(can, Config){
	
	return can.Model({
		findAll: 'GET /'+Config.api+'/news/feeds',
		findOne: 'GET /'+Config.api+'/news/{id}',
		create: 'POST /'+Config.api+'/news',
		update: 'PUT /'+Config.api+'/news/{id}',
		destroy: 'DELETE /'+Config.api+'/news/{id}'
	}, {});
	
});

