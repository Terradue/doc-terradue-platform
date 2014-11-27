
define(['can', 'geobrowser/config'], function(can, Config){
	
	return can.Model({
		findAll: 'GET /'+Config.api+'/datapackage',
		findOne: 'GET /'+Config.api+'/datapackage/{Id}',
		create: 'POST /'+Config.api+'/datapackage',
		destroy: 'DELETE /'+Config.api+'/service/wps/job/{Id}'
	}, {});
	
});

