
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		findAll: 'GET /'+Config.api+'/user',
		findOne: 'GET /'+Config.api+'/user/{id}/admin',
		create: 'POST /'+Config.api+'/user',
		update: 'PUT /'+Config.api+'/user',
		destroy: 'DELETE /'+Config.api+'/user/{id}',

		createArtifactoryDomain: function(data){
			return $.post('/'+Config.api+'/user/repository/group', data);
//			return $.ajax('/'+Config.api+'/user/repository/group', {
//				type : "POST",
//				dataType : "json",
//				data : {Id:id}
//			});
		},

		getRepositories: function(data){
			return $.get('/'+Config.api+'/user/repository?format=json');
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

