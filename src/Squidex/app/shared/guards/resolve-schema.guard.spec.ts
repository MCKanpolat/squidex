/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { SchemasService } from 'shared';

import { ResolveSchemaGuard } from './resolve-schema.guard';
import { RouterMockup } from './router-mockup';

describe('ResolveSchemaGuard', () => {
    const route = {
        params: {
            appName: 'my-app'
        },
        parent: {
            params: {
                schemaName: 'my-schema'
            }
        }
    };

    let appsStore: IMock<SchemasService>;

    beforeEach(() => {
        appsStore = Mock.ofType(SchemasService);
    });

    it('should navigate to 404 page if schema is not found', (done) => {
        appsStore.setup(x => x.getSchema('my-app', 'my-schema'))
            .returns(() => Observable.of(null));
        const router = new RouterMockup();

        const guard = new ResolveSchemaGuard(appsStore.object, <any>router);

        guard.resolve(<any>route, null)
            .then(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should navigate to 404 page if schema loading fails', (done) => {
        appsStore.setup(x => x.getSchema('my-app', 'my-schema'))
            .returns(() => Observable.throw(null));
        const router = new RouterMockup();

        const guard = new ResolveSchemaGuard(appsStore.object, <any>router);

        guard.resolve(<any>route, null)
            .then(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should return schema if loading succeeded', (done) => {
        const schema = {};

        appsStore.setup(x => x.getSchema('my-app', 'my-schema'))
            .returns(() => Observable.of(schema));
        const router = new RouterMockup();

        const guard = new ResolveSchemaGuard(appsStore.object, <any>router);

        guard.resolve(<any>route, null)
            .then(result => {
                expect(result).toBe(schema);

                done();
            });
    });
});