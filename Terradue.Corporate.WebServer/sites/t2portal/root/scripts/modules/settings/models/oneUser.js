
define(['can', 'config'], function(can, Config){
	
	return can.Model({
		findOne: 'GET /'+Config.api+'/one/user/current',
		create: 'POST /'+Config.api+'/one/user',
		update: 'PUT /'+Config.api+'/one/user',
		
		setOnePassword: function(data, success, fail){
			var def = new this(data).save();
			def.then(success).fail(fail);
			return def;
		},

		createOneUser: function(data, success, fail){
			var def = new this(data).save();
			def.then(success).fail(fail);
			return def;
		}
	}, {});
	
});