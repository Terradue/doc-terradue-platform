
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		findOne: 'GET /'+Config.api+'/cert',
		create: 'POST /'+Config.api+'/cert',
		update: 'PUT /'+Config.api+'/user',
		destroy: 'DELETE /'+Config.api+'/cert',
	}, {});
	
});
