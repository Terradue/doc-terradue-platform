
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		findAll: 'GET /'+Config.api+'/user',
		findOne: 'GET /'+Config.api+'/user/{id}',
		create: 'POST /'+Config.api+'/user',
		update: 'PUT /'+Config.api+'/user',
		destroy: 'DELETE /'+Config.api+'/user/{id}'
	}, {});
	
});

