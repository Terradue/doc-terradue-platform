

define({
	template: 'mustache',
	errorNotify: true,
	errorLog: false,
	mainContainer: '#mainContainer',
	page404: "modules/pages/views/404.html",
//	api: "t2api",
	
	staticPages: {
		'about': '/static/about.html',
		'boh': '/boh.html',
		'contact': {
			url: '/static/contact.html',
			data: [
				{id:1, name: 'pinco', surname: 'pallino'},
				{id:2, name: 'tizio', surname: 'caio'},
				{id:3, name: 'sempronio', surname: 'bah'},
				{id:4, name: 'ciccio', surname: 'ceras', username:'ciccio'},
			],
			fnLoad: function(){
				// alert('loaded');
			}
		},
		'contact/ciccio': '/static/contact/ciccio.html',
	},
	
});