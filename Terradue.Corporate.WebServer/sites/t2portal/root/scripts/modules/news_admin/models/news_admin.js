
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		findAll: 'GET /'+Config.api+'/news',
//		findOne: 'GET /'+Config.api+'/wps/{Id}',
		create: 'POST /'+Config.api+'/news',
		update: 'PUT /'+Config.api+'/news',
		destroy: 'DELETE /'+Config.api+'/news/{Id}'
	}, {});
	
});

