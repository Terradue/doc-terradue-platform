
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		
		signin: function(username, password){
			return $.post('/'+Config.api+'/oauth', {
				username: username,
				password: password,
				ajax: true
			});
		}
	
	//create: 'POST /'+Config.api+'/user/registration',

		
	}, {});
	
});
