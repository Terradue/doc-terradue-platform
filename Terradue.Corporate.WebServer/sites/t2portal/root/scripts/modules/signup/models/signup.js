

define(['can', 'config'], function(can, Config){
	
	return can.Model({
		create: 'POST /'+Config.api+'/user/registration',

		registerUser: function(userData){
			return $.ajax('/'+Config.api+'/user/registration', {
				type: 'POST',
				data: userData,
				dataType: 'json'
			});
		}

	}, {});
	
});
