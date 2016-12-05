
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		findAll: 'GET /'+Config.api+'/user',
		findOne: 'GET /'+Config.api+'/user/{id}/admin',
		create: 'POST /'+Config.api+'/user',
		update: 'PUT /'+Config.api+'/user',
		destroy: 'DELETE /'+Config.api+'/user/{id}',

		updateT2username: function(data){
			return $.post('/'+Config.api+'/user/username', data);
		},

		createLdapDomain: function(data){
			return $.post('/'+Config.api+'/user/ldap/domain', data);
		},

		createArtifactoryDomain: function(data){
			return $.post('/'+Config.api+'/user/repository/group', data);
		},

		createCatalogueIndex: function(data){
			return $.post('/'+Config.api+'/user/catalogue/index?format=json', data);
		},

		getRepositories: function(id){
			return $.get('/'+Config.api+'/user/repository?format=json' + (id != 0 ? "&id="+id : ""));
		},

		createRepository: function(data){
			return $.post('/'+Config.api+'/user/repository', data);
//			return $.ajax('/'+Config.api+'/user/repository/group', {
//				type : "POST",
//				dataType : "json",
//				data : {Id:id}
//			});
		},

	}, {});
	
});

