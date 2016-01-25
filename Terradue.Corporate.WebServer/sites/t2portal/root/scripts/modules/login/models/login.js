
define(['can', 'config'], function(can, Config){

	return can.Model({
		// defaults
		//findAll: 'GET /'+Config.api+'/user/current',
		findOne: 'GET /'+Config.api+'/user/current',
		create: 'GET /'+Config.api+'/auth',
		destroy: 'DELETE /'+Config.api+'/auth',
		
		// custom actions: login and logout
		login: function(user, callback){
			return this.create(user, callback);
			
//			return $.get('/'+Config.api+'/login?username='+user.username+'&password='+user.password+'&format=json', callback);
		},
		logout: function(callback){
			return this.destroy().then(callback);
		},
		isLogged: function(callback){
			return this.findOne({}, callback);
			//return this.findAll(function(result){});
		}
	}, {});

});
