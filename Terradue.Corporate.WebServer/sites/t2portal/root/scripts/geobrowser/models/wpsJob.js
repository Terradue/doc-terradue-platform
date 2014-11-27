
define(['can', 'geobrowser/config'], function(can, Config){
	
	return can.Model({
		findAll: 'GET /'+Config.api+'/service/wps/job',
		create: 'POST /'+Config.api+'/service/wps/job',
		destroy: 'DELETE /'+Config.api+'/service/wps/job/{id}'
	}, {});
	
});

