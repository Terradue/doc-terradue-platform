
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		update: 'PUT /'+Config.api+'/user',
	}, {});
	
});
