
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		findAll: 'GET /'+Config.api+'/plans',
		create: 'POST /'+Config.api+'/user/upgrade',
		update: 'POST /'+Config.api+'/user/upgrade',
		
		upgrade: function(data){
			return $.post('/'+Config.api+'/user/upgrade', data);
		}
	}, {});
	
});
