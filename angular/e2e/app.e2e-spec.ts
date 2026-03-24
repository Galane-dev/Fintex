import { FintexTemplatePage } from './app.po';

describe('Fintex App', function () {
    let page: FintexTemplatePage;

    beforeEach(() => {
        page = new FintexTemplatePage();
    });

    it('should display message saying app works', () => {
        page.navigateTo();
        expect(page.getParagraphText()).toEqual('app works!');
    });
});
