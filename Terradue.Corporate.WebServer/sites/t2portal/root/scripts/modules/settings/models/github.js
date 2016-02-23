
define(['jquery', 'can', 'config'], function($, can, Config){
	
	return can.Model({
		update: 'PUT /'+Config.api+'/github/user',
		findOne: 'GET /'+Config.api+'/github/user/current',
		
		postSshKey: function(){
			return $.ajax('/'+Config.api+'/github/sshkey', {
				type : "POST",
				dataType : "json",
				data : {}
			});	
		},
		
		getGithubToken: function(password){
			return $.ajax('/'+Config.api+'/github/token', {
				type : "PUT",
				dataType : "json",
				data : {Password:password, Scope:"write:public_key", Description:"Terradue Sandboxes Application"}
			});
		},
	}, {});
	
});
