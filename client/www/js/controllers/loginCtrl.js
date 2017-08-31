(function() {
'use strict';

    angular
        .module('starter.controllers')
        .controller('loginController', loginController);

    loginController.$inject = ['LoginService', '$state', 'api', '$ionicPopup', '$scope', 'promiseError', '$ionicLoading'];
    function loginController(LoginService, $state, api, $ionicPopup, $scope, promiseError, $ionicLoading) {
        var vm = this;

        vm.dados = {
            email: '',
            senha: '',
            codigoRec: '',
            novaSenha: '',
            confirmaSenha: ''
        };
        vm.erro = false;
        vm.erroMsg = '';
        
        vm.fazerLogin = fazerLogin;
        vm.loginGoogle = loginGoogle;

        activate();

        //////////////// Public

        function activate() {

        }

        function fazerLogin() {
            startLoading();
            LoginService.doLogin(vm.dados.email, vm.dados.senha)
                .then(loginSuccess, promiseError.rejection)
                .catch(promiseError.exception);
        }

        function loginGoogle() {
            startLoading();
            LoginService.gDialog()
                .then(LoginService.gLogin, promiseError.rejection)
                .then(loginSuccess, promiseError.rejection)
                .catch(promiseError.exception);
        }

        //////////////// Private

        function startLoading() {
            $ionicLoading.show({
                content: 'Carregando',
                animation: 'fade-in',
                showBackdrop: true,
                maxWidth: 200,
                showDelay: 0
            });
        }

        function loginSuccess() {
            $state.go('app.tela_inicial');
        }
    }
})();
