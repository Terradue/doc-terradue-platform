
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		
		signin: function(signinInfo){
			return $.post('/'+Config.api+'/oauth?ajax=true', signinInfo);
		},

		signInEverest: function(){
			return $.get('/'+Config.api+'/oauth/everest');
		},
		
		consent: function(consentInfo){
			return $.ajax({
				type: 'POST',
				url: '/'+Config.api+'/oauth?ajax=true',
				contentType : 'application/json;charset=UTF-8',
				data: consentInfo,
				dataType: 'json'
			});
		},
		
		denyConsent: function(){
			return $.ajax({
				type: 'DELETE',
				url: '/'+Config.api+'/oauth?ajax=true'
			});
		}
	
	//create: 'POST /'+Config.api+'/user/registration',

		
	}, {});
	
});
