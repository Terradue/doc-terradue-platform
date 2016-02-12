
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		
		signin: function(username, password){
			return $.post('/'+Config.api+'/oauth', {
				username: username,
				password: password,
				ajax: true
			});
		},
		
		consent: function(consentInfo, successHandler){
			return $.ajax({
				type: 'POST',
				url: '/'+Config.api+'/oauth?ajax=true',
				contentType : 'application/json;charset=UTF-8',
				data: consentInfo,
				dataType: 'json',
				statusCode: {
					204: function(data, textStatus, jqXHR){
						var location = jqXHR.getResponseHeader('Location');
						successHandler(location, jqXHR);
					}
				}
			});
		}
	
	//create: 'POST /'+Config.api+'/user/registration',

		
	}, {});
	
});
