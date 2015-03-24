

define(['can', 'config'], function(can, Config){
	
	return can.Model({
		create: 'POST /'+Config.api+'/user/registration',
	}, {});
	
});
