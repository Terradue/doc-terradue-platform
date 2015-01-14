

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

		partners: '/static/partners.html',
		tour: '/static/tour.html',
		resources: '/static/resources.html',
		
		people: {
			url: '/static/people.html',
			data: [
				{id:1, name: 'Gonçalves', surname: 'Pedro', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:2, name: 'Brito', surname: 'Fabrice', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:3, name: 'Maiozzi', surname: 'Francesca', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:4, name: 'Loeschau', surname: 'Frank', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:5, name: 'D\'Andria', surname: 'Fabio', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:6, name: 'Barchetta', surname: 'Francesco', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:7, name: 'Mathot', surname: 'Emmanuel', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:8, name: 'Caumont', surname: 'Hervé', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:9, name: 'Boissier', surname: 'Enguerran', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:10, name: 'Rossi', surname: 'Cesare', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:11, name: 'Cerasuolo', surname: 'Francesco', image: '/modules/pages/img/corporate/terradue_2.0.png'},
				{id:12, name: 'D\'Andelis', surname: 'Costanzo', image: '/modules/pages/img/corporate/terradue_2.0.png'},
			],
			fnLoad: function(){
				// eventual javascript stuff
			}
		}

		

	},
	
});