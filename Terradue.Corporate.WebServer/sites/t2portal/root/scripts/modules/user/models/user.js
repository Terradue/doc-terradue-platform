
define(['can', 'configurations/user'], function(can, Config){
	
	return can.Model({
		findAll: 'GET /'+Config.api+'/user',
		findOne: 'GET /'+Config.api+'/user/{id}',
		create: 'POST /'+Config.api+'/user',
		update: 'PUT /'+Config.api+'/user/{id}',
		destroy: 'DELETE /'+Config.api+'/user/{id}'
	}, {});
	
});

