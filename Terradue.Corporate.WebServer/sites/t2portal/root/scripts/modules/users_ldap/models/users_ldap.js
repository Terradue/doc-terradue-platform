
define(['can', 'config'], function(can, Config){

	UsersLdapModel = can.Model.extend({
	 	id: "Username"
	},{});

	return UsersLdapModel({
		findAll: 'GET /'+Config.api+'/user/ldap'
	}, {});
	
});

