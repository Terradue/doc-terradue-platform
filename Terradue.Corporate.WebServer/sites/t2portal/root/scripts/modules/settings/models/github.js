
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		update: 'PUT /'+Config.api+'/github/user',
		findOne: 'GET /'+Config.api+'/github/user/current',
		
		postSshKey: function(successCallback, failCallback){
			var aj = $.post('/'+Config.api+'/github/sshkey', successCallback, 'json');
			aj.fail(failCallback);
			return aj;				
		},
		
		getGithubToken: function(password, successCallback, failCallback){
			$.ajax('/'+Config.api+'/github/token', {
				type : "PUT",
				dataType : "json",
				data : {Password:password, Scope:"write:public_key", Description:"Terradue Sandboxes Application"},
				success: successCallback,
				error: failCallback,
			});
		},
	}, {});
	
});
