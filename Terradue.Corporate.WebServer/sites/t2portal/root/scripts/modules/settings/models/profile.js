
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		update: 'PUT /'+Config.api+'/user',

		changeEmail: function(email){
			return $.ajax('/'+Config.api+'/user/email', {
				type : "PUT",
				dataType : "json",
				data : {Email:email}
			});
		},
	}, {});
	
});
