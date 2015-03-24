

define(['can', 'config'], function(can, Config){
	
	return can.Model({
		create: 'PUT /'+Config.api+'/user/passwordreset',
	}, {});
	
});
