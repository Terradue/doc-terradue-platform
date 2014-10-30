

define({
	template: 'mustache',
	errorNotify: true,
	errorLog: false,
	mainContainer: '#mainContainer',
	page404: "modules/pages/views/404.html",
	firstPage: '/static/home.html',
	api: "t2api",
	
	staticPages: {
		about: '/static/about.html',
		people: '/static/people.html',
		partners: '/static/partners.html',
		tour: '/static/tour.html',
		resources: '/static/resources.html',
		
		samplePeople: {
			url: '/static/sample.html',
			data: [
				{id:1, name: 'pinco', surname: 'pallino'},
				{id:2, name: 'tizio', surname: 'caio'},
				{id:3, name: 'sempronio', surname: 'bah'},
				{id:4, name: 'ciccio', surname: 'ceras'},
			],
			fnLoad: function(){
				// eventual javascript stuff
			}
		}
	},
	
});