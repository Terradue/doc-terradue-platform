
define({
	selector: '#myMap',
	layoutSelector: '#mapLayoutContainer',
	template: 'mustache',
	
	api: 't2api',
	
	opensearchUrl: 'https://data.terradue.com/ec/catalogue/manu/dataset/description',
//	opensearchUrl: "fakeOpensearch/description.xml",
//	opensearchUrl: "http://grid-eo-catalog.esrin.esa.int/catalogue/gpod/MER_RR__1P/description",
	
	densityMapEnabled: true,
	
	wpsPollingTime: 2000,
	
	wpsJobsPerPage: 10,
	
	contexts: [
		{
			id: 'EOData',
			name: 'EO data',
			search: {
				searchTerms: 'eo data',
			},
		},{
			id: 'EOResultsStandard',
			name: 'EO results (standard)',
			search: {
				searchTerms: 'standard',
			}
		},{
			id: 'EOResultsAdvanced',
			name: 'EO results (advanced)',
			search: {
				searchTerms: 'advanced',
			}
		},{
			id: 'Publications',
			name: 'Publications',
			search: {
				searchTerms: 'publications',
			}
		},{
			id: 'Community',
			name: 'Community',
			search: {
				searchTerms: 'community',
			}
		}
	]
});
