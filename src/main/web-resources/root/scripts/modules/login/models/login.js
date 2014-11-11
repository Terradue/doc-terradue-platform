
define(['can', 'config'], function(can, Config){

	return can.Model({
		//findAll: 'GET /'+Config.api+'/user/current',
		findOne: 'GET /'+Config.api+'/user/current',
		create: 'POST /'+Config.api+'/auth',
//		create: 'POST /'+Config.api+'/crowd/auth',
		destroy: 'DELETE /'+Config.api+'/auth',
		
		// custom actions: login and logout
		login: function(user, callback){
			return this.create(user, callback);
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
